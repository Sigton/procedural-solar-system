using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationSettings : ScriptableObject
{

    // the types of bodies that exist
    public enum BodyType { Star, Planet, Moon }

    // editor layout
    [Header("General")]
    public string settingsName;
    public LightingSettings lightingSettings;

    [Header("Star Settings")]
    public StarSettings starSettings;

    [Header("Planet Settings")]
    public PlanetSettings[] planetSettings;


    // a quick class to hold all of the lighting settings for illuminating planets
    [System.Serializable]
    public class LightingSettings
    {
        public float distanceFromSource = 60;
        [Range(0.0001f, 0.01f)]
        public float intensityFactor = 1;
    }

    // class definitions for each type of setting
    // with a parent class that just gives each settings its own id
    [System.Serializable]
    public class CelestialBodySettings
    {
        [HideInInspector]
        public int id;
        [HideInInspector]
        public BodyType bodyType;

        [Header("Generic Properties")]
        public float surfaceGravity;
        public float radius;
        public float axialSpin;
        public float axisTilt;

        [Header("Appearance Settings")]
        public ColorSettings colorSettings;
        public StructureSettings structureSettings;

        public CelestialBodySettings()
        {
            id = SettingsIdentifier.Instance.nextID;
        }
    }

    [System.Serializable]
    public class StarSettings
    {
        [HideInInspector]
        public int id;
        [HideInInspector]
        public BodyType bodyType;

        [Header("Generic Properties")]
        public float surfaceGravity;
        public float radius;
        public float axialSpin;
        public float axisTilt;

        [Header("Star Properties")]
        public float temperature;

        public StarSettings()
        {
            id = SettingsIdentifier.Instance.nextID;
            bodyType = BodyType.Star;
        }
    }

    [System.Serializable]
    public class PlanetSettings : CelestialBodySettings
    {
        [Header("Planet Motion")]
        public float distance;
        [Range(0.3f, 0.9f)]
        public float eccentricity = 0.71f;
        [Range(0f, 360f)]
        public float offsetAngle;
        [Header("Orbit Tracing")]
        public bool drawPath = false;
        public int pathLength;
        [Header("Moons")]
        public MoonSettings[] moonSettings;

        public PlanetSettings()
        {
            bodyType = BodyType.Planet;
        }
    }

    [System.Serializable]
    public class MoonSettings : CelestialBodySettings
    {
        [Header("Moon Motion")]
        public float distance;
        [Range(0.3f, 0.9f)]
        public float eccentricity = 0.71f;
        [Range(0f, 360f)]
        public float offsetAngle;
        [Header("Orbit Tracing")]
        public bool drawPath = false;
        public int pathLength;

        public MoonSettings ()
        {
            bodyType = BodyType.Moon;
        }
    }
}
