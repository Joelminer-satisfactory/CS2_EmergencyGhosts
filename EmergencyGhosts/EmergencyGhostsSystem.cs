using Colossal.Entities;
using Colossal.Logging;
using EmergencyGhosts;
using Game;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Scripting;

public partial class EmergencyGhostsSystem : GameSystemBase
{

    private EntityQuery m_VehicleQuery;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();

        // Defines the filter for entities this system cares about
        m_VehicleQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                ComponentType.ReadWrite<CarNavigation>(),
                ComponentType.ReadWrite<Blocker>(),
                ComponentType.ReadOnly<CarCurrentLane>(),
                ComponentType.ReadOnly<Car>(),
                ComponentType.ReadOnly<PrefabRef>()
            },
            None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
            }
        });

        RequireForUpdate(m_VehicleQuery);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        if (!Mod.m_Setting.Enabled)
        {
            return;
        }

        // Grabs all entities matching the query into a temporary array
        NativeArray<Entity> entities = m_VehicleQuery.ToEntityArray(Allocator.TempJob);
        EntityManager em = EntityManager;

        for (int i = 0; i < entities.Length; i++)
        {
            Entity entity = entities[i];

            // Only process if the car is a Police Car, Fire Engine, or Ambulance
            if (!em.HasComponent<Game.Vehicles.PoliceCar>(entity) &&
                !em.HasComponent<Game.Vehicles.FireEngine>(entity) &&
                !em.HasComponent<Game.Vehicles.Ambulance>(entity))
            {
                continue;
            }

            Car car = em.GetComponentData<Car>(entity);
            Game.Vehicles.Ambulance ambulance = em.GetComponentData<Game.Vehicles.Ambulance>(entity);
            Game.Vehicles.PoliceCar policeCar = em.GetComponentData<Game.Vehicles.PoliceCar>(entity);
            Game.Vehicles.FireEngine fireEngine = em.GetComponentData<Game.Vehicles.FireEngine>(entity);
            if (((int)car.m_Flags & 1u) == 0 && ((int)ambulance.m_State & 4u) == 0 && ((int)ambulance.m_State & 2u) == 0)
            {
                if (Mod.m_Setting.EmergencyOnly)
                {
                    continue;
                }
            }

            if (em.HasComponent<Game.Vehicles.FireEngine>(entity) && fireEngine.m_TargetRequest != Entity.Null && em.HasComponent<FireRescueRequest>(fireEngine.m_TargetRequest))
            {
                FireRescueRequest fireRescueRequest = em.GetComponentData<FireRescueRequest>(fireEngine.m_TargetRequest);
                //still not right
                if (em.HasComponent<Car>(fireRescueRequest.m_Target) || em.HasComponent<Road>(fireRescueRequest.m_Target))
                {
                    continue;
                }
            }
            if (em.HasComponent<Game.Vehicles.PoliceCar>(entity) && policeCar.m_TargetRequest != Entity.Null && em.HasComponent<FireRescueRequest>(policeCar.m_TargetRequest))
            {
                PoliceEmergencyRequest policeEmergencyRequest = em.GetComponentData<PoliceEmergencyRequest>(policeCar.m_TargetRequest);
                //still not right
                //TODO: find correct component type
                if (em.HasComponent<Car>(policeEmergencyRequest.m_Target) || em.HasComponent<Road>(policeEmergencyRequest.m_Target))
                {
                    continue;
                }
            }
            
            

            Blocker blocker = em.GetComponentData<Blocker>(entity);
            CarCurrentLane currentLane = em.GetComponentData<CarCurrentLane>(entity);

            if (ShouldClearBlocker(currentLane, blocker))
            {
                CarNavigation navigation = em.GetComponentData<CarNavigation>(entity);
                PrefabRef prefabRef = em.GetComponentData<PrefabRef>(entity);

                // Speed calculation logic
                float targetSpeed = CalculateLaneMaxSpeed(currentLane, prefabRef)/2;
                float currentSpeed = math.abs(navigation.m_MaxSpeed);
                float minSpeed = math.max(5f, targetSpeed * (Mod.m_Setting.SpeedMultiplier/100));

                // Gradually increase speed (10% + 1) up to the lane's target speed
                float restoredSpeed = math.min(targetSpeed, math.max(minSpeed, currentSpeed * 1.1f + 1f));
                float sign = math.sign(navigation.m_MaxSpeed);

                if (sign == 0f) sign = 1f;

                navigation.m_MaxSpeed = sign * restoredSpeed;
                em.SetComponentData(entity, navigation);

                // Clear the blocking entity so the vehicle can drive through it
                blocker.m_Blocker = Entity.Null;
                blocker.m_Type = (BlockerType)0;
                blocker.m_MaxSpeed = byte.MaxValue;
                em.SetComponentData(entity, blocker);
            }
        }
        entities.Dispose();
    }

    private bool ShouldClearBlocker(CarCurrentLane currentLane, Blocker blocker)
    {
        if (blocker.m_Blocker == Entity.Null)
        {
            return false;
        }

        // Do not clear if the blockage is a specific type (None, Continuing, Crossing,	Signal,	Temporary, Limit, Caution, Spawn, Oncoming)
        if ((int)blocker.m_Type == 5 || (int)blocker.m_Type == 6 || (int)blocker.m_Type == 7)
        {
            return false;
        }

        // Check specific lane flags (0x10 is parking space)
        if (((int)currentLane.m_LaneFlags & 0x10) != 0)
        {
            return false;
        }

        EntityManager em = EntityManager;

        // Never ghost through trains
        if (em.HasComponent<Train>(blocker.m_Blocker))
        {
            return false;
        }

        // Ghost through bicycles
        if (em.HasComponent<Bicycle>(blocker.m_Blocker))
        {
            return true;
        }

        // Ghost through humans and animals (creatures)
        if (em.HasComponent<Human>(blocker.m_Blocker) || em.HasComponent<Creature>(blocker.m_Blocker))
        {
            return true;
        }

        return true;
    }

    private float CalculateLaneMaxSpeed(CarCurrentLane currentLane, PrefabRef prefabRef)
    {
        EntityManager em = EntityManager;

        if (!em.HasComponent<CarData>(prefabRef.m_Prefab))
        {
            return 20f;
        }

        CarData carData = em.GetComponentData<CarData>(prefabRef.m_Prefab);

        if (em.HasComponent<Game.Net.CarLane>(currentLane.m_Lane))
        {
            Game.Net.CarLane carLane = em.GetComponentData<Game.Net.CarLane>(currentLane.m_Lane);
            // Factor in speed limits and how curvy the road is
            return math.min(CalculateMaxDriveSpeed(carData, carLane.m_SpeedLimit, carLane.m_Curviness), carData.m_MaxSpeed);
        }

        return math.min(15f, carData.m_MaxSpeed);
    }

    private bool IsEmergencyVehicle(Entity vehicleEntity)
    {
        EntityManager em = EntityManager;
        return em.HasComponent<Game.Vehicles.PoliceCar>(vehicleEntity) ||
               em.HasComponent<Game.Vehicles.FireEngine>(vehicleEntity) ||
               em.HasComponent<Game.Vehicles.Ambulance>(vehicleEntity);
    }

    private float CalculateMaxDriveSpeed(CarData carData, float speedLimit, float curviness)
    {
        if (curviness < 0.001f)
        {
            return speedLimit;
        }

        // Standard physics-based turning speed calculation
        float turningSpeed = carData.m_Turning.x * carData.m_MaxSpeed / math.max(1E-06f, curviness * carData.m_MaxSpeed + carData.m_Turning.x - carData.m_Turning.y);
        turningSpeed = math.max(1f, turningSpeed);
        return math.min(speedLimit, turningSpeed);
    }

    public EmergencyGhostsSystem()
    {
    }
}