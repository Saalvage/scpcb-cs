using System.Numerics;

namespace scpcb.Collision; 

public static class CollideRRR {
    public enum PointRelation {
        ON,
        BELOW,
        ABOVE,
    };

    public struct Collision {
        public bool Hit;
        public float CoveredAmount;
        public Vector3 Begin;
        public Vector3 End;
        public Vector3 Normal;
        public Vector3 SurfaceNormal;
    }

    //v0,v1 = edge verts
    //pn = poly normal
    //en = edge normal
    public static Collision EdgeTest(Vector3 v0, Vector3 v1, Vector3 pn, Vector3 en, Vector3 begin, Vector3 end, float radius) {
        Collision retVal = default;
        retVal.Hit = false;

        Vector3 tm0 = Extensions.SafeNormalize(en);
        Vector3 tm1 = Extensions.SafeNormalize(v1 - v0);
        Vector3 tm2 = Extensions.SafeNormalize(pn);

        Vector3 tm0Transposed = new Vector3(tm0.X, tm1.X, tm2.X);
        Vector3 tm1Transposed = new Vector3(tm0.Y, tm1.Y, tm2.Y);
        Vector3 tm2Transposed = new Vector3(tm0.Z, tm1.Z, tm2.Z);

        Vector3 sv = begin - v0;
        sv = new Vector3(Vector3.Dot(sv, tm0),Vector3.Dot(sv, tm1),Vector3.Dot(sv, tm2));
        Vector3 dv = end - v0;
        dv = new Vector3(Vector3.Dot(dv, tm0),Vector3.Dot(dv, tm1),Vector3.Dot(dv, tm2)) - sv;

        //do cylinder test...
        float a, b, c, d, t1, t2, t;
        a=(dv.X * dv.X + dv.Z * dv.Z);
        if( a == 0 ) { return retVal; }                    //ray parallel to cylinder
        b=(sv.X* dv.X+sv.Z* dv.Z)*2;
            c=(sv.X* sv.X+sv.Z* sv.Z)-radius* radius;
        d=b* b-4* a* c;
            if(d<0 ) { return retVal; }                    //ray misses cylinder
            t1 = (-b + MathF.Sqrt(d)) / (2 * a);
        t2 = (-b - MathF.Sqrt(d)) / (2 * a);
        t = t1 < t2 ? t1 : t2;
        if (t > 1) { return retVal; }    //intersects too far away
        Vector3 i = sv + dv * t;
        Vector3 p = new();
        if (i.Y > Vector3.Distance(v0, v1)) { return retVal; }    //intersection above cylinder
        if (i.Y >= 0) {
            p.Y = i.Y;
        } else {
            //below bottom of cylinder...do sphere test...
            a = Vector3.Dot(dv, dv);
            if (a == 0) { return retVal; }                //ray parallel to sphere
            b = Vector3.Dot(sv, dv) * 2;
            c = Vector3.Dot(sv, sv) - radius * radius;
            d = b * b - 4 * a * c;
            if (d < 0) { return retVal; }                //ray misses sphere
            t1 = (-b + MathF.Sqrt(d)) / (2 * a);
            t2 = (-b - MathF.Sqrt(d)) / (2 * a);
            t = t1 < t2 ? t1 : t2;
            if (t > 1) { return retVal; }
            i = sv + dv * t;
        }

        Vector3 n = i - p;
        n = new Vector3(Vector3.Dot(n, tm0Transposed), Vector3.Dot(n, tm1Transposed), Vector3.Dot(n, tm2Transposed));
        return Update(begin, end, t, n);
    }

    public static Collision Update(Vector3 begin, Vector3 end, float t, Vector3 n) {
        Collision retVal = default;
        retVal.Hit = false;

        if (t < 0 || t > 1) { return retVal; }

        Plane p = Helpers.PlaneFromNormalAndPoint(n, (end - begin) * t + begin);
        if (Vector3.Dot(p.Normal, (end - begin)) >= 0) { return retVal; }
        if (p.OnPlane(begin) == PointRelation.ON) { return retVal; }

        retVal.Begin = begin;
        retVal.End = end;
        retVal.CoveredAmount = t;
        retVal.Normal = Extensions.SafeNormalize(n);
        retVal.SurfaceNormal = retVal.Normal;
        retVal.Hit = true;
        return retVal;
    }

    public static Collision TriangleCollide(Vector3 begin, Vector3 end, float radius, Vector3 v0, Vector3 v1, Vector3 v2) {
        Collision retVal = default;
        retVal.Hit = false;

        //triangle plane
        Plane p = Helpers.PlaneFromPoints(v0, v1, v2);

        //move plane out
        p.D+=radius;
        float t = 1;

        //edge planes
        Plane p0 = Helpers.PlaneFromPoints(v0 + p.Normal, v1, v0 );
        Plane p1 = Helpers.PlaneFromPoints(v1 + p.Normal, v2, v1 );
        Plane p2 = Helpers.PlaneFromPoints(v2 + p.Normal, v0, v2 );

        if (Vector3.Dot(p.Normal, end - begin) < 0) {
            t = -p.EvalAtPoint(begin)/Vector3.Dot(p.Normal, (end - begin));

            //intersects triangle?
            Vector3 i = (end - begin) * t + begin;

            float eval0 = p0.EvalAtPoint(i);
            float eval1 = p1.EvalAtPoint(i);
            float eval2 = p2.EvalAtPoint(i);

            if (eval0>=0 && eval1>=0 && eval2>=0) {
                return Update(begin, end, t, p.Normal);
            }
        }
        //if (t<0 || t>1) { return retVal; }

        if (radius <= 0) { return retVal; }

        Collision temp;
        temp = EdgeTest(v0, v1, p.Normal, p0.Normal, begin, end, radius);
        if (temp.Hit) {
            retVal = temp;
        }
        temp = EdgeTest(v1, v2, p.Normal, p1.Normal, begin, end, radius);
        if (temp.Hit) {
            if (!retVal.Hit || retVal.CoveredAmount > temp.CoveredAmount) {
                retVal = temp;
            }
        }
        temp = EdgeTest(v2, v0, p.Normal, p2.Normal, begin, end, radius);
        if (temp.Hit) {
            if (!retVal.Hit || retVal.CoveredAmount > temp.CoveredAmount) {
                retVal = temp;
            }
        }

        retVal.SurfaceNormal = p.Normal;
        return retVal;
    }

    public static Collision TriangleCollide(Vector3 begin, Vector3 end, float height, float radius, Vector3 v0, Vector3 v1, Vector3 v2) {
        if (height <= radius) {
            return TriangleCollide(begin, end, radius, v0, v1, v2);
        }
        
        Collision retVal = default;
        retVal.Hit = false;
        
        var forward = end - begin;
        var upVector = new Vector3(0,1,0); // TODO: Not correct for noclip

        if (Math.Abs(Vector3.Dot(Extensions.SafeNormalize(forward), upVector)) > 0.9999f) {
            forward.X = 0; forward.Z = 0;
            var newBegin = begin;
            var newEnd = begin + forward;

            if (forward.Y > 0) {
                newBegin.Y -= height * 0.5f - radius;
                newEnd.Y += height * 0.5f - radius;
            } else {
                newBegin.Y += height * 0.5f - radius;
                newEnd.Y -= height * 0.5f - radius;
            }

            retVal = TriangleCollide(newBegin, newEnd, radius, v0, v1, v2);

            if (retVal.Hit) {
                Vector3 diff = (newEnd - newBegin) * retVal.CoveredAmount + newBegin - begin;
                if (forward.Y > 0) {
                    diff.Y -= height * 0.5f - radius;
                } else {
                    diff.Y += height * 0.5f - radius;
                }
                retVal.Begin = begin;
                retVal.End = end;
                retVal.CoveredAmount = diff.Length()/Vector3.Distance(end, begin);

                if (retVal.CoveredAmount > 1) {
                    retVal.Hit = false;
                }
                if (retVal.CoveredAmount < 0) {
                    retVal.CoveredAmount = 0;
                }
            }

            return retVal;
        }

        Vector3 forwardXZ = new Vector3(forward.X, 0, forward.Z);
        Vector3 planePoint = Extensions.SafeNormalize(forwardXZ) * radius;

        Vector3 planeNormal = Extensions.SafeNormalize(Vector3.Cross(Vector3.Cross(upVector, forward), forward));
        if (planeNormal.Y < 0) { planeNormal = -planeNormal; }

        Plane bottomPlane = Helpers.PlaneFromNormalAndPoint(planeNormal, begin + new Vector3(0, -height*0.5f + radius,0) + planePoint);
        Plane topPlane = Helpers.PlaneFromNormalAndPoint(planeNormal, begin + new Vector3(0, height*0.5f - radius,0) + planePoint);

        //cylinder collision
        //we only check for edges here because the sphere collision can handle the other case just fine
        Plane p = Helpers.PlaneFromPoints(v0,v1,v2);

        Collision temp;
        temp.Hit = false;

        Span<Vector3> edgePoints = stackalloc Vector3[6] { v0,v1,v1,v2,v2,v0 };
        
        var newBegin2 = new Vector3(begin.X, 0, begin.Z);
        var newEnd2 = new Vector3(end.X, 0, end.Z);

        for (int i=0;i<6;i+=2) {
            var edgeBegin = edgePoints[i];
            var edgeEnd = edgePoints[i+1];

            bool intersectsTop = topPlane.Intersects(edgeBegin, edgeEnd, out var edgePoint0, out var coveredAmountTop, true, true);
            bool intersectsBottom = bottomPlane.Intersects(edgeBegin, edgeEnd, out var edgePoint1, out var coveredAmountBottom, true, true);
            if (intersectsTop && intersectsBottom) {
                if (coveredAmountTop < 0) {
                    edgePoint0 = edgeBegin;
                } else if (coveredAmountTop > 1) {
                    edgePoint0 = edgeEnd;
                }

                if (coveredAmountBottom < 0) {
                    edgePoint1 = edgeBegin;
                } else if (coveredAmountBottom > 1) {
                    edgePoint1 = edgeEnd;
                }
            } else {
                if (topPlane.EvalAtPoint(edgeBegin) <=0 && bottomPlane.EvalAtPoint(edgeEnd) >=0) {
                    edgePoint0 = edgeBegin;
                    edgePoint1 = edgeEnd;
                } else {
                    continue;
                }
            }

            if (Vector3.DistanceSquared(edgePoint0, edgePoint1) < 0.0000001f) {
                continue;
            }

            edgePoint0.Y = 0; edgePoint1.Y = 0;

            Plane edgePlane = Helpers.PlaneFromPoints(edgePoint0 + upVector,edgePoint1,edgePoint0);

            temp = EdgeTest(edgePoint0,edgePoint1,upVector,edgePlane.Normal,newBegin2,newEnd2,radius);
            if (temp.Hit) {
                if (!retVal.Hit || retVal.CoveredAmount>temp.CoveredAmount) {
                    retVal = temp;
                }
            }
        }

        var bottomSphereBegin = begin;
        var bottomSphereEnd = end;
        bottomSphereBegin.Y -= height*0.5f-radius;
        bottomSphereEnd.Y -= height*0.5f-radius;
        var topSphereBegin = begin;
        var topShereEnd = end;
        topSphereBegin.Y += height*0.5f-radius;
        topShereEnd.Y += height*0.5f-radius;

        Collision bottomCollision = TriangleCollide(bottomSphereBegin, bottomSphereEnd, radius, v0, v1, v2);
        Collision topCollision = TriangleCollide(topSphereBegin, topShereEnd, radius, v0, v1, v2);

        if (bottomCollision.Hit && (!retVal.Hit || bottomCollision.CoveredAmount < retVal.CoveredAmount)) {
            retVal = bottomCollision;
            retVal.Begin = begin;
            retVal.End = end;
        }
        if (topCollision.Hit && (!retVal.Hit || topCollision.CoveredAmount < retVal.CoveredAmount)) {
            retVal = topCollision;
            retVal.Begin = begin;
            retVal.End = end;
        }

        if (retVal.Hit) {
            retVal.SurfaceNormal = p.Normal;
            retVal.Begin = begin;
            retVal.End = end;
        }

        return retVal;
    }

    public static Vector3 TryMove(Vector3 from, Vector3 to, float height, float radius, CollisionMeshCollection cmc) {
        int iterations = 0;
        Vector3 targetDir = Extensions.SafeNormalize(to - from);
        Vector3 currDir = targetDir;
        while (true) {
            Collision coll = cmc.Collide(from, to, height, radius);
            if (coll.Hit) {
                Vector3 resultPos = from + (to - from) * (coll.CoveredAmount * 0.995f);
                if (Vector3.DistanceSquared(resultPos, from) < 0.0001f) {
                    resultPos = from;
                    coll.CoveredAmount = 0;
                }
                from = resultPos;
                if (iterations >= 5) { break; }
                iterations++;
                float remainingDist = (to - from).Length();
                if (coll.Normal.Y <= 0.71f) {
                    //surface is too steep to climb up or pushes you down
                    coll.Normal.Y = 0;
                    coll.Normal = Extensions.SafeNormalize(coll.Normal);
                }
                Vector3 reflectedDir = Vector3.Reflect(currDir, coll.Normal) * -remainingDist;
                Plane p = Helpers.PlaneFromNormalAndPoint(coll.Normal, from);
                Vector3 tempPos = from;
                tempPos = tempPos + reflectedDir;
                tempPos = tempPos - p.Normal * (Vector3.Dot((tempPos - from), p.Normal) * 0.995f);
                currDir = Extensions.SafeNormalize(tempPos - from);
                if (Vector3.Dot(currDir, targetDir) < 0 && iterations > 1) { break; }
                to = tempPos;
            } else {
                from = to;
                break;
            }
        }
        return from;
    }
}
