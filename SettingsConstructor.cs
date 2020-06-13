using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsConstructor
{

    RandomGenerationSettings randomSettings;
    NoiseSettingsGenerator noiseSettingsGenerator;

    public SettingsConstructor(RandomGenerationSettings randomSettings)
    {
        this.randomSettings = randomSettings;
        noiseSettingsGenerator = new NoiseSettingsGenerator(randomSettings.randomNoiseSettings);
}

    public GenerationSettings CreateSettingsOject()
    {
        // we set up a GenerationSettings object that holds all of the information required for our solar system
        GenerationSettings newSettings = (GenerationSettings)ScriptableObject.CreateInstance("GenerationSettings");
        newSettings.settingsName = "New Settings";

        // set up the lighting settings
        newSettings.lightingSettings = randomSettings.lightingSettings;

        // set up the star settings
        // choose random numbers within the ranges specified in the random settings

        newSettings.starSettings = new GenerationSettings.StarSettings
        {
            surfaceGravity = Lerp(randomSettings.minStarSurfaceGravity, randomSettings.maxStarSurfaceGravity, UnityEngine.Random.value),
            radius = Lerp(randomSettings.minStarRadius, randomSettings.maxStarRadius, UnityEngine.Random.value),
            axialSpin = Lerp(randomSettings.minAxialSpin, randomSettings.maxAxialSpin, UnityEngine.Random.value) * Mathf.Sign(UnityEngine.Random.value - 0.5f),
            axisTilt = Lerp(randomSettings.minAxialTilt, randomSettings.maxAxialTilt, UnityEngine.Random.value),
            temperature = Lerp(randomSettings.minTemperature, randomSettings.maxTemperature, UnityEngine.Random.value)
        };

        // repeat for the planets, only there may be multiple planets
        int numPlanets = (int)Lerp(randomSettings.minNumPlanets, randomSettings.maxNumPlanets + 1, UnityEngine.Random.value);
        newSettings.planetSettings = new GenerationSettings.PlanetSettings[numPlanets];

        // as more planets are generated we want them to be increasing distances from the star
        // so we keep track of how far they are already from the star
        float distanceFromSun = Lerp(randomSettings.minInitDistanceFromSun, randomSettings.maxInitDistanceFromSun, UnityEngine.Random.value);

        for (int i = 0; i < numPlanets; i++)
        {

            // we need to generate the moons settings array before we populate the planet settings
            int numMoons = (int)Lerp(randomSettings.minNumMoons, randomSettings.maxNumMoons + 1, UnityEngine.Random.value);
            var moonSettings = new GenerationSettings.MoonSettings[numMoons];
            float distanceFromPlanet = Lerp(randomSettings.minInitDistanceFromPlanet, randomSettings.maxInitDistanceFromPlanet, UnityEngine.Random.value);

            // iterate through each moon and populate the moon settings
            for (int j = 0; j < numMoons; j++)
            {
                moonSettings[j] = new GenerationSettings.MoonSettings
                {
                    surfaceGravity = Lerp(randomSettings.minMoonSurfaceGravity, randomSettings.maxMoonSurfaceGravity, UnityEngine.Random.value),
                    radius = Lerp(randomSettings.minMoonRadius, randomSettings.maxMoonRadius, UnityEngine.Random.value),
                    axialSpin = Lerp(randomSettings.minAxialSpin, randomSettings.maxAxialSpin, UnityEngine.Random.value) * Mathf.Sign(UnityEngine.Random.value - 0.5f),
                    axisTilt = Lerp(randomSettings.minAxialTilt, randomSettings.maxAxialTilt, UnityEngine.Random.value),
                    structureSettings = CreateRandomStructureSettings(),
                    colorSettings = CreateColourSettings(),
                    distance = distanceFromPlanet,
                    eccentricity = Lerp(randomSettings.minOrbitEccentricity, randomSettings.maxOrbitEccentricity, UnityEngine.Random.value),
                    offsetAngle = Lerp(0, 360, UnityEngine.Random.value),
                    drawPath = randomSettings.traceOrbits,
                    pathLength = randomSettings.tracingLength
                };
                distanceFromPlanet *= 1 + Lerp(randomSettings.minMultDistanceFromPlanet, randomSettings.maxMultDistanceFromPlanet, UnityEngine.Random.value);
            }

            // now we can populate the planet settings
            newSettings.planetSettings[i] = new GenerationSettings.PlanetSettings
            {
                surfaceGravity = Lerp(randomSettings.minPlanetSurfaceGravity, randomSettings.maxPlanetSurfaceGravity, UnityEngine.Random.value),
                radius = Lerp(randomSettings.minPlanetRadius, randomSettings.maxPlanetRadius, UnityEngine.Random.value),
                axialSpin = Lerp(randomSettings.minAxialSpin, randomSettings.maxAxialSpin, UnityEngine.Random.value) * Mathf.Sign(UnityEngine.Random.value - 0.5f),
                axisTilt = Lerp(randomSettings.minAxialTilt, randomSettings.maxAxialTilt, UnityEngine.Random.value),
                structureSettings = CreateRandomStructureSettings(),
                colorSettings = CreateColourSettings(),
                distance = distanceFromSun,
                eccentricity = Lerp(randomSettings.minOrbitEccentricity, randomSettings.maxOrbitEccentricity, UnityEngine.Random.value),
                offsetAngle = Lerp(0, 360, UnityEngine.Random.value),
                drawPath = randomSettings.traceOrbits,
                pathLength = randomSettings.tracingLength,
                moonSettings = moonSettings
            };
            distanceFromSun *= 1 + Lerp(randomSettings.minMultDistanceFromSun, randomSettings.maxMultDistanceFromSun, UnityEngine.Random.value);
        }
        return newSettings;
    }

    StructureSettings CreateRandomStructureSettings()
    {
        StructureSettings newStructureSettings = new StructureSettings();

        // set the resolution of the meshes created for each celestial body
        newStructureSettings.resolution = randomSettings.defaultResolution;

        int numNoiseLayers = (int)Lerp(randomSettings.randomNoiseSettings.minNumNoiseLayers, randomSettings.randomNoiseSettings.maxNumNoiseLayers + 1, UnityEngine.Random.value);
        StructureSettings.NoiseLayer[] newNoiseLayers = new StructureSettings.NoiseLayer[numNoiseLayers];

        for (int i = 0; i < numNoiseLayers; i++)
        {
            newNoiseLayers[i] = new StructureSettings.NoiseLayer();

            // create a new type of noise settings
            NoiseSettings newNoiseSettings = new NoiseSettings();
            newNoiseSettings.filterType = (NoiseSettings.FilterType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(NoiseSettings.FilterType)).Length);
            if (i > 0)
            {
                newNoiseLayers[i].useFirstLayerAsMask = true;
            }
            // populate the settings depending on which type of noise it is
            if (newNoiseSettings.filterType == NoiseSettings.FilterType.Simple)
            {
                noiseSettingsGenerator.PopulateSimpleNoiseSettings(ref newNoiseSettings.simpleNoiseSettings, newNoiseLayers[i].useFirstLayerAsMask);
            } else if (newNoiseSettings.filterType == NoiseSettings.FilterType.Rigid)
            {
                noiseSettingsGenerator.PopulateRigidNoiseSettings(ref newNoiseSettings.rigidNoiseSettings, newNoiseLayers[i].useFirstLayerAsMask);
            }

            newNoiseLayers[i].noiseSettings = newNoiseSettings;
        }

        newStructureSettings.noiseLayers = newNoiseLayers;

        return newStructureSettings;
    }

    StructureSettings CreateStarStructure()
    {
        // stars are basically just spheres
        StructureSettings newStructureSettings = new StructureSettings();

        newStructureSettings.resolution = randomSettings.defaultResolution;
        newStructureSettings.noiseLayers = new StructureSettings.NoiseLayer[1];

        StructureSettings.NoiseLayer newNoiseLayer = new StructureSettings.NoiseLayer();
        NoiseSettings newNoiseSettings = new NoiseSettings();
        newNoiseSettings.filterType = (NoiseSettings.FilterType.Simple);
        newNoiseSettings.simpleNoiseSettings = new NoiseSettings.SimpleNoiseSettings();
        newNoiseSettings.simpleNoiseSettings.strength = 0;
        newNoiseLayer.noiseSettings = newNoiseSettings;

        newStructureSettings.noiseLayers[0] = newNoiseLayer;

        return newStructureSettings;
    }

    ColorSettings CreateColourSettings()
    {
        ColorSettings newColorSettings = new ColorSettings();
        newColorSettings.gradient = CreateRandomGradient();
        newColorSettings.material = new Material(Shader.Find("Shader Graphs/Planet"));
        return newColorSettings;
    }

    Gradient CreateRandomGradient()
    {
        Gradient newGradient = new Gradient();

        // for now just have the gradient interpolate between 2 colours
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        colorKey[1].time = 1.0f;

        // we want the colours fully opaque
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        newGradient.SetKeys(colorKey, alphaKey);

        return newGradient;
    }

    public static float Lerp(float v0, float v1, float t)
    {
        return v0 + t * (v1 - v0);
    }
}
