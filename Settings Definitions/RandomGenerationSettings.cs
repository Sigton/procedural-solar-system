using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RandomGenerationSettings : ScriptableObject
{

    [Header("General")]
    public Material defaultMaterial;
    public Mesh defaultMesh;

    public float minOrbitEccentricity = 0.3f;
    public float maxOrbitEccentricity = 0.9f;

    public float minAxialSpin = -4f;
    public float maxAxialSpin = 4f;

    public float minAxialTilt = -90f;
    public float maxAxialTilt = 90f;

    [Header("Lighting")]
    public GenerationSettings.LightingSettings lightingSettings;

    [Header("Star Settings")]
    public float minStarSurfaceGravity = 18.0f;
    public float maxStarSurfaceGravity = 25.0f;

    public float minStarRadius = 5.0f;
    public float maxStarRadius = 15.0f;

    public float minTemperature = 0f;
    public float maxTemperature = 1f;

    [Header("Planet Settings")]
    public int minNumPlanets = 0;
    public int maxNumPlanets = 10;

    public float minPlanetSurfaceGravity = 0.1f;
    public float maxPlanetSurfaceGravity = 4.0f;

    public float minPlanetRadius = 0.5f;
    public float maxPlanetRadius = 3.0f;

    public float minInitDistanceFromSun = 40.0f;
    public float maxInitDistanceFromSun = 60.0f;

    public float minMultDistanceFromSun = 0.7f;
    public float maxMultDistanceFromSun = 2.0f;

    [Header("Moon Settings")]
    public int minNumMoons = 0;
    public int maxNumMoons = 3;

    public float minMoonSurfaceGravity = 0.01f;
    public float maxMoonSurfaceGravity = 0.2f;

    public float minMoonRadius = 0.5f;
    public float maxMoonRadius = 1.5f;

    public float minInitDistanceFromPlanet = 8.0f;
    public float maxInitDistanceFromPlanet = 12.0f;

    public float minMultDistanceFromPlanet = 0.7f;
    public float maxMultDistanceFromPlanet = 1.5f;

    [Header("Orbit Tracing Settings")]
    public bool traceOrbits;
    public int tracingLength = 1;

    [Header("Terrain Generation")]
    public int defaultResolution = 10;

    [Header("Noise Options")]
    public RandomNoiseSettings randomNoiseSettings;

    [System.Serializable]
    public class RandomNoiseSettings
    {
        public int minNumNoiseLayers;
        public int maxNumNoiseLayers;

        [Header("General Noise")]
        public float minStrength;
        public float maxStrength;

        public int minNumLayers;
        public int maxNumLayers;

        public float minBaseRoughness;
        public float maxBaseRoughness;

        public float minRoughness;
        public float maxRoughness;
    }
}
