﻿using System.Numerics;
using BepuPhysics;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Graphics.Models;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;
using SCPCB.Scenes;
using SCPCB.Serialization;
using SCPCB.Utility;

namespace SCPCB.Map.Entities;

public class Prop : Disposable, IMapEntity, IEntityHolder, ISerializableEntity {
    private record PropData(string File, Transform Transform, BodyVelocity Velocity, bool IsStatic) : SerializableData {
        protected override ISerializableEntity DeserializeImpl(GraphicsResources gfxRes, IScene scene, IReferenceResolver refResolver) {
            var prop = new Prop(scene.GetEntitiesOfType<PhysicsResources>().Single(), File, Transform, IsStatic, false);
            if (prop.Model is DynamicPhysicsModel pmc) {
                pmc.Body.Velocity = Velocity;
            }
            return prop;
        }
    }

    private readonly string _file;

    public PhysicsModel Model { get; }

    public Prop(PhysicsResources physics, string file, Transform transform, bool isStatic = true,
        bool needsPositionAdjustment = true) {
        _file = file;
        var template = physics.ModelCache.GetModel(file).CreateDerivative();
        if (needsPositionAdjustment) {
            // The object origin in B3D is at the bottom.
            template.Shape.ComputeBounds(transform.Rotation, out var min, out var max);
            // TODO: This is still not entirely correct.
            transform.Position += new Vector3(0, -min.Y * transform.Scale.Y, 0);
        }
        if (isStatic) {
            Model = template.InstantiatePhysicsStatic();
        } else {
            Model = template.InstantiatePhysicsDynamic(1);
        }
        Model.WorldTransform = transform;
    }

    public IEnumerable<IEntity> Entities {
        get {
            yield return Model;
        }
    }

    protected override void DisposeImpl() {
        Model.Dispose();
    }

    public SerializableData SerializeImpl() {
        var dpm = Model as DynamicPhysicsModel;
        return new PropData(_file, Model.WorldTransform, dpm?.Body.Velocity ?? default, dpm == null);
    }
}
