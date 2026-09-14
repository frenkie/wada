// /**
//  * Created by Pawel Homenko on  12/2023
//  */

using UnityEngine;

namespace NatureManufacture.RAM
{
    [CreateAssetMenu(fileName = "FenceProfile", menuName = "NatureManufacture/FenceProfile", order = 1)]
    public class WaterfallProfile : ScriptableObject, IProfile<WaterfallProfile>
    {
        [HideInInspector] [SerializeField] private GameObject gameObject;

        [SerializeField] private float triangleDensity = 8f;

        [field: SerializeField] public Material WaterfallMaterial { get; set; }
        [field: SerializeField] public float SimulationTime { get; set; } = 10;
        [field: SerializeField] public float TimeStep { get; set; } = 0.1f;

        [field: SerializeField] public float BaseStrength { get; set; } = 10f;
        [field: SerializeField] public Vector2 UvScale { get; set; } = new(0.1f, 0.03f);

        [field: SerializeField] public float RestitutionCoeff { get; set; } = 0.1f;
        [field: SerializeField] public float RestitutionAnglelerp { get; set; } = 0f;
        [field: SerializeField] public LayerMask RaycastMask { get; set; } = ~0;


        [field: SerializeField] public float BlurStrength { get; set; } = 2f;
        [field: SerializeField] public int BlurIterations { get; set; } = 2;
        [field: SerializeField] public int BlurSize { get; set; } = 2;
        [field: SerializeField] public float MaxWaterfallDistance { get; set; } = 20;
        [field: SerializeField] public float MinPointDistance { get; set; } = 0.5f;

        [field: SerializeField] public float TerrainOffset { get; set; } = 0.1f;

        [field: SerializeField] public AnimationCurve AlphaByDistance { get; set; } = AnimationCurve.Constant(0, 1, 1);

        [field: SerializeField] public float FloatSpeed { get; set; } = 10;


        public float TriangleDensity
        {
            get => triangleDensity;
            set => triangleDensity = value;
        }

        public GameObject GameObject
        {
            get => gameObject;
            set => gameObject = value;
        }


        public void SetProfileData(WaterfallProfile otherProfile)
        {
            WaterfallMaterial = otherProfile.WaterfallMaterial;
            TriangleDensity = otherProfile.TriangleDensity;
            SimulationTime = otherProfile.SimulationTime;
            BaseStrength = otherProfile.BaseStrength;
            TimeStep = otherProfile.TimeStep;
            UvScale = otherProfile.UvScale;
            RestitutionCoeff = otherProfile.RestitutionCoeff;
            RestitutionAnglelerp = otherProfile.RestitutionAnglelerp;
            RaycastMask = otherProfile.RaycastMask;
            BlurStrength = otherProfile.BlurStrength;
            BlurIterations = otherProfile.BlurIterations;
            BlurSize = otherProfile.BlurSize;
            MaxWaterfallDistance = otherProfile.MaxWaterfallDistance;
            MinPointDistance = otherProfile.MinPointDistance;
            TerrainOffset = otherProfile.TerrainOffset;
            AlphaByDistance = otherProfile.AlphaByDistance;
            FloatSpeed = otherProfile.FloatSpeed;
        }

        public bool CheckProfileChange(WaterfallProfile otherProfile)
        {
            if (WaterfallMaterial != otherProfile.WaterfallMaterial)
                return true;
            if (TriangleDensity != otherProfile.TriangleDensity)
                return true;
            if (SimulationTime != otherProfile.SimulationTime)
                return true;
            if (BaseStrength != otherProfile.BaseStrength)
                return true;
            if (TimeStep != otherProfile.TimeStep)
                return true;
            if (UvScale != otherProfile.UvScale)
                return true;
            if (RestitutionCoeff != otherProfile.RestitutionCoeff)
                return true;
            if (RestitutionAnglelerp != otherProfile.RestitutionAnglelerp)
                return true;
            if (RaycastMask != otherProfile.RaycastMask)
                return true;
            if (BlurStrength != otherProfile.BlurStrength)
                return true;
            if (BlurIterations != otherProfile.BlurIterations)
                return true;
            if (BlurSize != otherProfile.BlurSize)
                return true;
            if (MaxWaterfallDistance != otherProfile.MaxWaterfallDistance)
                return true;
            if (MinPointDistance != otherProfile.MinPointDistance)
                return true;
            if (TerrainOffset != otherProfile.TerrainOffset)
                return true;
            if (FloatSpeed != otherProfile.FloatSpeed)
                return true;

            return false;
        }
    }
}