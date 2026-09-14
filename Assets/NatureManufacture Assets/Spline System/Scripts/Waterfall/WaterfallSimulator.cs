using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace NatureManufacture.RAM
{
    public class WaterfallSimulator
    {
        private Waterfall _waterfall;

        public struct MeshSimulation
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Binormal;
            public Vector3 Tangent;
            public float DistanceU;
            public float DistanceV;
            public Vector3 Velocity;
            public float Distance;
        }

        public List<List<MeshSimulation>> AllSimulationPoints { get; set; } = new();
        private List<Vector3> CurrentVelocities { get; set; } = new();

        private bool _isLooping = false;


        public List<List<MeshSimulation>> MakeSimulation(Waterfall waterfall)
        {
            _waterfall = waterfall;
            InitializeInitialData();
            DoSimulation();
            CleanSimulation(waterfall);
            return AllSimulationPoints;
        }


        private void CleanSimulation(Waterfall waterfall)
        {
            List<List<MeshSimulation>> cleanedSimulation = new();

            MeshSimulation meshSimulation;
            for (int i = 0; i < AllSimulationPoints.Count; i++)
            {
                cleanedSimulation.Add(new List<MeshSimulation>());
                meshSimulation = AllSimulationPoints[i][0];
                meshSimulation.Distance = 0;
                cleanedSimulation[i].Add(meshSimulation);
            }

            for (int i = 0; i < AllSimulationPoints.Count; i++)
            {
                // cleanedSimulation.Add(new List<MeshSimulation>());
                float dist = 0;
                for (int p = 1; p < AllSimulationPoints[i].Count; p++)
                {
                    meshSimulation = AllSimulationPoints[i][p];
                    if (Vector3.Distance(cleanedSimulation[i][^1].Position, meshSimulation.Position) > waterfall.BaseProfile.MinPointDistance)
                    {
                        dist += Vector3.Distance(cleanedSimulation[i][^1].Position, meshSimulation.Position);
                        meshSimulation.Distance = dist;
                        cleanedSimulation[i].Add(meshSimulation);
                    }


                    if (dist > waterfall.BaseProfile.MaxWaterfallDistance)
                        break;
                }

                //Debug.Log("Distance: " + dist);
            }

            AllSimulationPoints = cleanedSimulation;
        }

        private void DoSimulation()
        {
            Vector3 position = _waterfall.transform.position;
            HashSet<int> pointsHit = new();
            int t = 0;
            //Debug.Log(System.Convert.ToString(_waterfall.BaseProfile.RaycastMask.value,2));
            // Iterating over simulation time
            for (float time = 0; time <= _waterfall.BaseProfile.SimulationTime; time += _waterfall.BaseProfile.TimeStep)
            {
                for (int i = 0; i < _waterfall.NmSpline.Points.Count; i++)
                {
                    if (pointsHit.Contains(i))
                        continue;

                    MeshSimulation currentPosition = CalculateMeshSimulation(AllSimulationPoints, i, t, position, out Vector3 newVelocity, out MeshSimulation newMeshSimulation);


                    Ray ray = new(currentPosition.Position + position, newVelocity.normalized);

                   
                    // Check if the raycast hits an object
                    if (Physics.Raycast(ray, out RaycastHit hit, newVelocity.magnitude * _waterfall.BaseProfile.TimeStep, _waterfall.BaseProfile.RaycastMask, QueryTriggerInteraction.Ignore))
                    {
                        //Debug.Log($"hit {hit.collider.name}");
                        Vector3 reflectedVector = Vector3.Reflect(newVelocity, hit.normal);
                        reflectedVector *= _waterfall.BaseProfile.RestitutionCoeff;


                        newVelocity = Vector3.Lerp(reflectedVector, new Vector3(newVelocity.x, reflectedVector.y, newVelocity.z), _waterfall.BaseProfile.RestitutionAnglelerp);

                        if (_waterfall.BaseDebug)
                        {
                            Debug.DrawRay(hit.point, hit.normal, Color.magenta, 5f);
                            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 5f);
                            Debug.DrawRay(hit.point, reflectedVector, Color.cyan, 5f);
                        }

                        newMeshSimulation.Position = hit.point - position; // + hit.normal * _waterfall.BaseProfile.TerrainOffset;
                        newMeshSimulation.Velocity = newVelocity;

                        //Debug.Log(newVelocity.magnitude);

                        //pointsHit.Add(i);
                    }

                    newMeshSimulation.Position += newMeshSimulation.Normal * _waterfall.BaseProfile.TerrainOffset; // + Vector3.up * 0.1f;


                    CurrentVelocities[i] = newVelocity;
                    AllSimulationPoints[i].Add(newMeshSimulation);
                }

                BlurVelocities();
                t++;

                // If all raycasts hit an object, end simulation
                if (pointsHit.Count == _waterfall.NmSpline.Points.Count)
                    break;
            }
        }

        private MeshSimulation CalculateMeshSimulation(List<List<MeshSimulation>> allSimulationPoints, int i, int t, Vector3 position, out Vector3 newVelocity, out MeshSimulation newMeshSimulation)
        {
            MeshSimulation currentSimulation = allSimulationPoints[i][t];
            newVelocity = CurrentVelocities[i] + Physics.gravity * _waterfall.BaseProfile.TimeStep;
            //currentSimulation.Velocity = newVelocity;
            Vector3 velocity = newVelocity * _waterfall.BaseProfile.TimeStep;
            Vector3 newPosition = currentSimulation.Position + velocity;

            Vector3 testPosition = newPosition + position;

            /*if (Physics.Raycast(testPosition + Vector3.up, Vector3.down, out RaycastHit checkHitUnderTerrain, 3, _waterfall.BaseProfile.RaycastMask, QueryTriggerInteraction.Ignore))
            {
                if (checkHitUnderTerrain.point.y > testPosition.y)
                {
                    //  newPosition = checkHitUnderTerrain.point - position + 0.1f * Vector3.up;
                }
            }*/


            newMeshSimulation = new()
            {
                Position = newPosition,
                Normal = Vector3.Cross(velocity, currentSimulation.Binormal).normalized,
                Binormal = currentSimulation.Binormal,
                Tangent = velocity,
                DistanceU = currentSimulation.DistanceU + Vector3.Distance(newPosition, currentSimulation.Position),
                // DistanceV = i == 0 ? 0 : newSimulationPoints[^1].DistanceV + Vector3.Distance(newPosition, newSimulationPoints[^1].Position),
                DistanceV = currentSimulation.DistanceV,
                Velocity = newVelocity
            };
            allSimulationPoints[i][t] = currentSimulation;
            return currentSimulation;
        }


        private void BlurVelocities()
        {
            List<Vector3> blurredVelocities = new(CurrentVelocities.Count);

            for (int it = 0; it < _waterfall.BaseProfile.BlurIterations; it++)
            {
                for (int i = 0; i < CurrentVelocities.Count; i++)
                {
                    Vector3 blurredDirection = RamMath.GaussianBlur(CurrentVelocities, i, _waterfall.BaseProfile.BlurStrength, _waterfall.BaseProfile.BlurSize, _isLooping);
                    blurredVelocities.Add(blurredDirection);
                }

                if (_isLooping)
                    blurredVelocities[^1] = blurredVelocities[0];

                CurrentVelocities.Clear();
                CurrentVelocities.AddRange(blurredVelocities);
                blurredVelocities.Clear();
            }
        }


        private void InitializeInitialData()
        {
            CurrentVelocities.Clear();
            AllSimulationPoints.Clear();

            _isLooping = _waterfall.NmSpline.IsLooping;

            for (int i = 0; i < _waterfall.NmSpline.Points.Count; i++)
            {
                AllSimulationPoints.Add(new List<MeshSimulation>());
                Vector3 binormal = _waterfall.NmSpline.Points[i].Binormal;
                Quaternion orientation = _waterfall.NmSpline.Points[i].Orientation;

                AllSimulationPoints[i].Add(new MeshSimulation()
                {
                    Position = _waterfall.NmSpline.Points[i].Position,
                    Normal = _waterfall.NmSpline.Points[i].Normal,
                    Tangent = binormal,
                    Binormal = -_waterfall.NmSpline.Points[i].Tangent,
                    DistanceU = 0,
                    DistanceV = _waterfall.NmSpline.Points[i].Distance,
                });

                CurrentVelocities.Add((orientation * Vector3.right * _waterfall.BaseProfile.BaseStrength));
            }
        }
    }
}