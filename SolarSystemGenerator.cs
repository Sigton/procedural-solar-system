using System.Collections.Generic;
using UnityEngine;
// using System.Diagnostics;
using UnityEditor;

public class SolarSystemGenerator : MonoBehaviour
{
    public GenerationSettings settings;
    public RandomGenerationSettings randomSettings;

    List<CelestialBody> celestialBodies;
    CelestialBody star;

    [Range(0.1f, 10f)]
    public float simulationSpeed = 1;
    public bool autoUpdate = false;

    private void OnValidate()
    {
        celestialBodies = new List<CelestialBody>();
    }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        // first of all, clear out any old solar system thats kicking about
        DeleteSolarSystem();

        // we need to interpret the settings that have been passed in
        // and initialize data structures to prepare for terrain generation

        // there are two different ways of generating a solar system
        // you can pass in specific settings that include the properties of each star and planet etc
        // or you can pass in a number of ranges for each parameter, that a solar system is then randomly chosen out from
        
        // prioritise method #1
        if (settings == null)
        {
            // if we do not have specific settings for the solar system, we need to generate our own
            SettingsConstructor settingsConstructor = new SettingsConstructor(randomSettings);
            settings = settingsConstructor.CreateSettingsOject();
        }

        settings.name = settings.settingsName;
        
        // now that all the settings are set up, we can pass them into the solar system generator
        GenerateSolarSystem(settings);

        // then when all the objects are create we need to illuminate them
        SetUpUniverseLighting();
        
        // once the solar system has been generated we pass all the bodies into the camera
        try
        {
            Camera.main.GetComponent<CameraPosition>().SetCelestialBodies(celestialBodies.ToArray());
        } catch
        {
            // yes i know this is bad
            return;
        }
    
    }

    void DeleteSolarSystem()
    {
        int numChildren = transform.childCount;
        for (int i = 0; i < numChildren; i++)
        {
            if (Application.isEditor)
                DestroyImmediate(transform.GetChild(0).gameObject);
            else
                Destroy(transform.GetChild(0).gameObject);
        }
        if (celestialBodies is null)
        {
            celestialBodies = new List<CelestialBody>();
        }
        else
        {
            celestialBodies.Clear();
        }
    }

    void GenerateSolarSystem(GenerationSettings settings)
    {
        // procedurally generate a solar system
        // using the settings that were set up in the Generate function

        // initially create the star at the centre of the solar system
        CreateStar(settings.starSettings);

        // then create all the planets we need
        for (int i = 0; i < settings.planetSettings.Length; i++)
        {
            CreatePlanet(settings.planetSettings[i]);
            for (int j = 0; j < settings.planetSettings[i].moonSettings.Length; j++)
            {
                CreateMoon(settings.planetSettings[i].moonSettings[j], settings.planetSettings[i].id);
            }
        }
    }

    void CreateStar(GenerationSettings.StarSettings starSettings)
    {
        // create a new star gameobject
        // and make it a child of the solar system
        GameObject newStar = new GameObject
        {
            name = "Star"
        };
        newStar.transform.parent = transform;

        // then add components and set the settings
        CelestialBody cbComponent = newStar.AddComponent<CelestialBody>();

        // pass the settings into the celestial body to all it to set up
        cbComponent.SetUpBody(starSettings);

        star = cbComponent;
        celestialBodies.Add(cbComponent);
    }

    void CreatePlanet(GenerationSettings.PlanetSettings planetSettings)
    {
        // create a new planet gameobject
        // make it a child of the solar system
        GameObject newPlanet = new GameObject
        {
            name = "Planet " + transform.childCount.ToString()
        };
        newPlanet.transform.parent = transform;

        // add components to the planet
        CelestialBody cbComponent = newPlanet.AddComponent<CelestialBody>();

        // assign the values to the planet from the settings
        cbComponent.SetUpBody(planetSettings, true, star);

        // then register it as a celestial body
        celestialBodies.Add(cbComponent);
    }

    void CreateMoon(GenerationSettings.MoonSettings moonSettings, int planetID)
    {
        // find the planet that the moon will orbit
        CelestialBody planet = FindWithID(planetID);

        // create a new moon gameobject
        // make it a child of its parent planet
        GameObject newMoon = new GameObject
        {
            name = "Moon " + planet.transform.childCount.ToString()
        };
        newMoon.transform.parent = planet.transform;

        // add components
        CelestialBody cbComponent = newMoon.AddComponent<CelestialBody>();

        // assign the values to the moon from the settings
        cbComponent.SetUpBody(moonSettings, true, planet);

        // register as celestial body
        celestialBodies.Add(cbComponent);
    }

    void SetUpUniverseLighting()
    {
        // create a game object to hold all of the spotlights that illuminate the objects
        GameObject lightingHolder = new GameObject
        {
            name = "Body Lights"
        };
        // parent it to the star in the solar system
        lightingHolder.transform.parent = star.transform;
        lightingHolder.transform.position = Vector3.zero;

        // for each planet and moon
        foreach (CelestialBody body in celestialBodies)
        {
            if (body.bodyType != GenerationSettings.BodyType.Star)
            {
                // create a new spotlight centred in the sun
                GameObject newSpotLightObj = new GameObject
                {
                    name = body.name + " light"
                };
                newSpotLightObj.AddComponent<LightController>().Setup(body, star, settings.lightingSettings);

                // parent the new spotlight to the lighting holder
                newSpotLightObj.transform.parent = lightingHolder.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        // physics simulation
        float tickSpeed = Universe.physicsTimeStep * simulationSpeed;
        
        // first of all calculate the forces between bodies
        for (int i = 0; i < celestialBodies.Count; i++)
        {
            if (celestialBodies[i].bodyType != GenerationSettings.BodyType.Star)
                celestialBodies[i].UpdateVelocity(ref celestialBodies, tickSpeed);
        }

        // then calculate how their positions should move based on the previously calculated forces
        for (int i = 0; i < celestialBodies.Count; i++)
        {
            if (celestialBodies[i].bodyType != GenerationSettings.BodyType.Star)
                celestialBodies[i].UpdatePosition(tickSpeed);
        }
    }

    public void OnSettingsChanged()
    {
        if (autoUpdate)
            Generate();
    }

    public void OnCreateNewPresssed()
    {
        settings = null;
        Generate();
    }

    public void OnDeleteSettingsPressed()
    {
        settings = null;
        DeleteSolarSystem();
    }

    public void SaveSolarSystem()
    {
        if (!AssetDatabase.Contains(settings))
        {
            AssetDatabase.CreateAsset(settings, "Assets/Settings/SolarSystem-" + settings.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }

    private CelestialBody FindWithID(int id)
    {
        foreach (CelestialBody body in celestialBodies)
        {
            // Debug.Log(id + " " + body.gameObject.name + " " + body.ID);
            if (body.ID == id)
            {
                return body;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        // trace orbits for any planets that have that setting on
        TraceOrbits();
    }

    void TraceOrbits()
    {
        
        if (celestialBodies is null) return;
        foreach (CelestialBody body in celestialBodies)
        {
            if (body.drawPath)
            {
                body.OrbitLookahead(body.pathLength, ref celestialBodies, Universe.tracerTimeStep);
            }
        }
        
    }

    private void OnApplicationQuit()
    {
        Generate();
    }
}
