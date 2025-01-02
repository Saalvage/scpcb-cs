using System.Text;
using SCPCB.Utility;

namespace SCPCB.Map;

public class MapGenerator {
    // Taken largely verbatim from https://github.com/blitz-research/blitz3d/blob/master/bbruntime/bbmath.cpp
    public class B3DRandom {
        private static int SeedFromStr(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            var temp = 0;
            var shift = 0;
            foreach (var b in bytes) {
                temp ^= b << shift;
                shift = (shift + 1) % 24;
            }
            return temp;
        }

        private const int RND_A = 48271;
        private const int RND_M = 2147483647;
        private const int RND_Q = 44488;
        private const int RND_R = 3399;

        private int _state;

        public B3DRandom(int seed) {
            seed &= 0x7fffffff;
            _state = seed == 0 ? 1 : seed;
        }

        public B3DRandom(string str) : this(SeedFromStr(str)) { }

        public float NextSingle() {
            _state = RND_A * (_state % RND_Q) - RND_R * (_state / RND_Q);
            if (_state < 0) _state += RND_M;
            if (_state == 0) {
                _state = RND_R;
            }
            return (_state & 65535) / 65536.0f + (.5f / 65536.0f);
        }

        public float NextSingle(float from, float to = 0) {
            return NextSingle() * (to - from) + from;
        }

        public int NextInt(int from, int to = 1) {
            if (from > to) {
                (from, to) = (to, from);
            }
            return (int)NextSingle(from, to + 1);
        }
    }

    private static readonly string[] PRESET_SEEDS = [
        "NIL", "NO", "d9341", "5CP_I73", "DONTBLINK", "CRUNCH", "die", "HTAED", "rustledjim", "larry", "JORGE",
        "dirtymetal", "whatpumpkin",
    ];

    public static string GenerateRandomSeed()
        => Random.Shared.Next(15) == 0
            ? PRESET_SEEDS.RandomElement()
            : string.Concat(Enumerable.Range(0, Random.Shared.Next(4, 9))
                .Select(_ => Random.Shared.Next(3) == 0
                    ? (char)('0' + Random.Shared.Next(10))
                    : (char)('a' + Random.Shared.Next(26))));

    private readonly int _mapWidth;
    private readonly int _mapHeight;
    public const int ZONE_COUNT = 3;

    public MapGenerator(int mapWidth, int mapHeight) {
        // The original implementation results in maps that are always one less than the specified dimensions.
        _mapWidth = mapWidth + 1;
        _mapHeight = mapHeight + 1;
    }

    private int GetZone(int y) {
        return Math.Min((int)((float)(_mapWidth - y) / _mapWidth * ZONE_COUNT), ZONE_COUNT - 1);
    }

    public PlacedRoomInfo?[,] GenerateMap(IDictionary<Shape, RoomInfo[]> rooms, string seed) {
        Log.Information("Generating map from seed \"{Seed}\"", seed);

        var rng = new B3DRandom(seed);

        // The extra space allows for avoiding edge checks.
        var mapTemp = new byte[_mapWidth + 1, _mapHeight + 1];
        var x = _mapWidth / 2;
        var y = _mapHeight - 2;
        var temp = 0;

        for (var i = y; i < _mapHeight; i++) {
            mapTemp[x, i] = 1;
        }

        do {
            var width = rng.NextInt(10, 15);

            if (x > _mapWidth * 0.6) {
                width = -width;
            } else if (x > _mapWidth * 0.4) {
                x -= width / 2;
            }

            if (x + width > _mapWidth - 3) {
                width = _mapWidth - 3 - x;
            } else if (x + width < 2) {
                width = -x + 2;
            }

            x = Math.Min(x, x + width);
            width = Math.Abs(width);

            for (var i = x; i <= x + width; i++) {
                mapTemp[Math.Min(i, _mapWidth), y] = 1;
            }

            var height = rng.NextInt(3, 4);
            if (y - height < 1) {
                height = y - 1;
            }

            var yHallways = rng.NextInt(4, 5);

            if (GetZone(y - height) != GetZone(y - height + 1)) {
                height--;
            }

            for (var i = 0; i < yHallways; i++) {
                var x2 = Math.Max(Math.Min(rng.NextInt(x, x + width - 1), _mapWidth - 2), 2);

                while ((mapTemp[x2, y - 1] | mapTemp[x2 - 1, y - 1] | mapTemp[x2 + 1, y - 1]) != 0) {
                    x2++;
                }

                if (x2 < x + width) {
                    int tempHeight;
                    if (i == 0) {
                        tempHeight = height;
                        x2 = rng.NextInt(2) == 1 ? x : x + width;
                    } else {
                        tempHeight = rng.NextInt(1, height);
                    }

                    for (var y2 = y - tempHeight; y2 <= y; y2++) {
                        if (GetZone(y2) != GetZone(y2 + 1)) {
                            mapTemp[x2, y2] = 255;
                        } else {
                            mapTemp[x2, y2] = 1;
                        }
                    }

                    if (tempHeight == height) {
                        temp = x2;
                    }
                }
            }

            x = temp;
            y -= height;
        } while (y >= 2);

        var shapes = Enum.GetValues<Shape>().ToDictionary(x => x, _ => new int[ZONE_COUNT]);
        for (y = 1; y < _mapHeight; y++) {
            var z = GetZone(y);
            for (x = 1; x < _mapWidth; x++) {
                if (mapTemp[x, y] == 0) {
                    continue;
                }

                var neighbors = NeighborCount(x, y);
                if (mapTemp[x, y] != 255) {
                    mapTemp[x, y] = (byte)neighbors;
                }

                var shape = neighbors switch {
                    1 => Shape._1,
                    2 => DetermineShape2(x, y),
                    3 => Shape._3,
                    4 => Shape._4,
                };
                shapes[shape][z]++;
            }
        }

        // Force room1s.
        for (var z = 0; z < ZONE_COUNT; z++) {
            const int DESIRED_ROOM1_COUNT = 5;
            if (shapes[Shape._1][z] < DESIRED_ROOM1_COUNT) {
                for (y = (_mapHeight / ZONE_COUNT) * (2 - z) + 1;
                     y <= ((_mapHeight / ZONE_COUNT) * ((2 - z) + 1.0)) - 2;
                     y++) {

                    for (x = 2; x < _mapWidth - 1; x++) {
                        if (mapTemp[x, y] != 0 || NeighborCount(x, y) != 1) {
                            continue;
                        }

                        var (nx, ny) = Up(x, y) ? (x, y - 1)
                            : Down(x, y) ? (x, y + 1)
                            : Left(x, y) ? (x - 1, y)
                            : (x + 1, y);

                        switch (mapTemp[nx, ny]) {
                            default:
                                continue;
                            case 2 when DetermineShape2(nx, ny) == Shape._2:
                                shapes[Shape._2][z]--;
                                shapes[Shape._3][z]++;
                                break;
                            case 3:
                                shapes[Shape._3][z]--;
                                shapes[Shape._4][z]++;
                                break;
                        }

                        Log.Debug("Forced Room1 at ({X}, {Y})", x, y);
                        mapTemp[x, y] = 1;
                        mapTemp[nx, ny]++;
                        if (++shapes[Shape._1][z] >= DESIRED_ROOM1_COUNT) {
                            goto force_1_done;
                        }
                    }
                }
            }
            force_1_done:;
        }

        for (var z = 0; z < ZONE_COUNT; z++) {
            var (yStart, yEnd) = z switch {
                2 => (2, _mapHeight / ZONE_COUNT),
                1 => (_mapHeight / ZONE_COUNT + 1, (int)(_mapHeight * (2.0 / 3.0) - 1)),
                0 => ((int)(_mapHeight * (2.0 / 3.0) + 1), _mapHeight - 2),
            };

            // Force room4s.
            if (shapes[Shape._4][z] < 1) {
                for (y = yStart; y <= yEnd; y++) {
                    for (x = 2; x <= _mapWidth - 2; x++) {
                        if (mapTemp[x, y] != 3) {
                            continue;
                        }

                        var any = false;
                        if (!Right(x, y) && NeighborCount(x + 1, y) == 1) {
                            mapTemp[x + 1, y] = 1;
                            any = true;
                        } else if (!Left(x, y) && NeighborCount(x - 1, y) == 1) {
                            mapTemp[x - 1, y] = 1;
                            any = true;
                        } else if (!Down(x, y) && NeighborCount(x, y + 1) == 1) {
                            mapTemp[x, y + 1] = 1;
                            any = true;
                        } else if (!Up(x, y) && NeighborCount(x + 1, y) == 1) {
                            mapTemp[x, y - 1] = 1;
                            any = true;
                        }

                        if (any) {
                            Log.Debug("Forced Room4 at ({X}, {Y})", x, y);
                            mapTemp[x, y] = 4;
                            shapes[Shape._1][z]++;
                            shapes[Shape._4][z]++;
                            shapes[Shape._3][z]--;
                            goto force_4_done;
                        }
                    }
                }
                Log.Debug("Couldn't force Room4 into zone {Zone}", z);
            }
            force_4_done:;

            // Force room2Cs.
            if (shapes[Shape._2C][z] < 1) {
                yStart++;
                yEnd--;

                for (y = yStart; y <= yEnd; y++) {
                    for (x = 3; x <= _mapWidth - 3; x++) {
                        if (mapTemp[x, y] != 1) {
                            continue;
                        }

                        (int, int)? fulfilled = null;
                        if (Left(x, y) && NeighborCount(x + 1, y) == 1) {
                            if (NeighborCount(x + 1, y - 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x + 1, y] = 2;
                                mapTemp[x + 1, y - 1] = 1;
                                fulfilled = (x + 1, y);
                            } else if (NeighborCount(x + 1, y + 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x + 1, y] = 2;
                                mapTemp[x + 1, y + 1] = 1;
                                fulfilled = (x + 1, y);
                            }
                        } else if (Right(x, y) && NeighborCount(x - 1, y) == 1) {
                            if (NeighborCount(x - 1, y - 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x - 1, y] = 2;
                                mapTemp[x - 1, y - 1] = 1;
                                fulfilled = (x - 1, y);
                            } else if (NeighborCount(x - 1, y + 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x - 1, y] = 2;
                                mapTemp[x - 1, y + 1] = 1;
                                fulfilled = (x - 1, y);
                            }
                        } else if (Up(x, y) && NeighborCount(x, y + 1) == 1) {
                            if (NeighborCount(x - 1, y + 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x, y + 1] = 2;
                                mapTemp[x - 1, y + 1] = 1;
                                fulfilled = (x, y + 1);
                            } else if (NeighborCount(x + 1, y + 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x, y + 1] = 2;
                                mapTemp[x + 1, y + 1] = 1;
                                fulfilled = (x, y + 1);
                            }
                        } else if (Down(x, y) && NeighborCount(x, y - 1) == 1) {
                            if (NeighborCount(x - 1, y - 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x, y - 1] = 2;
                                mapTemp[x - 1, y - 1] = 1;
                                fulfilled = (x, y - 1);
                            } else if (NeighborCount(x + 1, y - 1) == 0) {
                                mapTemp[x, y] = 2;
                                mapTemp[x, y - 1] = 2;
                                mapTemp[x + 1, y - 1] = 1;
                                fulfilled = (x, y - 1);
                            }
                        }

                        if (fulfilled.HasValue) {
                            var (placedX, placedY) = fulfilled.Value;
                            Log.Debug("Forced Room2C at ({X}, {Y})", placedX, placedY);
                            shapes[Shape._2C][z]++;
                            shapes[Shape._2][z]++;
                            goto force_2c_done;
                        }
                    }
                }

                Log.Debug("Couldn't force Room2C into zone {Zone}", z);
            }
            force_2c_done:;
        }

        Shape DetermineShape2(int x, int y) => Up(x, y) && Down(x, y) || Left(x, y) && Right(x, y) ? Shape._2 : Shape._2C;
        int NeighborCount(int x, int y) => UpI(x, y) + DownI(x, y) + LeftI(x, y) + RightI(x, y);
        bool Up(int x, int y) => mapTemp[x, y - 1] != 0;
        bool Down(int x, int y) => mapTemp[x, y + 1] != 0;
        bool Left(int x, int y) => mapTemp[x - 1, y] != 0;
        bool Right(int x, int y) => mapTemp[x + 1, y] != 0;
        int UpI(int x, int y) => Up(x, y) ? 1 : 0;
        int DownI(int x, int y) => Down(x, y) ? 1 : 0;
        int LeftI(int x, int y) => Left(x, y) ? 1 : 0;
        int RightI(int x, int y) => Right(x, y) ? 1 : 0;

        var plac = new PlacedRoomInfo?[_mapWidth - 1, _mapHeight - 1];
        for (x = 0; x < mapTemp.GetLength(0); x++) {
            for (y = 0; y < mapTemp.GetLength(1); y++) {
                /*if (x < 1 || x > _mapWidth - 2 || y < 1 || y > _mapHeight - 1) {
                    Debug.Assert(mapTemp[x, y] == 0);
                }*/

                if (mapTemp[x, y] == 0) {
                    continue;
                }

                var shape = mapTemp[x, y] switch {
                    1 => Shape._1,
                    2 => DetermineShape2(x, y),
                    3 => Shape._3,
                    4 => Shape._4,
                    255 => Shape._2,
                };
                var dir = DetermineDirection(shape, x, y);
                plac[x, y - 1] = new(rooms[shape][rng.NextInt(0, rooms[shape].Length - 1)], dir);
            }
        }
        return plac;

        // TODO: Make sure these are accurate.
        Direction DetermineDirection(Shape shape, int x, int y)
            => shape switch {
                Shape._1
                    => Up(x, y) ? Direction.Down
                    : Down(x, y) ? Direction.Up
                    : Right(x, y) ? Direction.Left
                    : Direction.Right,
                Shape._2 => (Down(x, y) ? Direction.Up : Direction.Right).Rotate(rng.NextInt(2) == 1 ? 2 : 0),
                Shape._2C => Up(x, y) ? (Left(x, y) ? Direction.Down : Direction.Left) : (Left(x, y) ? Direction.Right : Direction.Up),
                Shape._3 => !Up(x, y) ? Direction.Up
                    : !Down(x, y) ? Direction.Down
                    : !Right(x, y) ? Direction.Right
                    : Direction.Left,
                Shape._4 => Direction.Up,
            };
    }
}
