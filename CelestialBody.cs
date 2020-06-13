using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;


public class CelestialBody : MonoBehaviour
{

    // everything that floats in space must have this class
    // this allows an object to be influenced by gravity

    // what type of celestial object is this
    public GenerationSettings.BodyType bodyType;

    public float mass;
    public float radius;
    public float surfaceGravity;
    public float surfaceTemperature;
    public Vector3 initialVelocity;

    float axisTilt;
    Vector3 localUpAxis;
    float axialSpinSpeed;

    Vector3 currentVelocity;
    CelestialBody orbitalParent;
    float orbitalParentMass;

    public Rigidbody rb;
    public bool drawPath = false;
    public int pathLength = 1;

    public GameObject visualSphere;

    public int ID { get; private set; }

    private void Awake()
    {
        currentVelocity = new Vector3();
        initialVelocity = new Vector3();
    }

    private void Start()
    {
        currentVelocity = initialVelocity;
    }

    void GenericSetup(GenerationSettings.CelestialBodySettings newSettings)
    {
        // add a rigidbody to the body
        if (rb is null) rb = gameObject.AddComponent<Rigidbody>();

        // set the type of body
        bodyType = newSettings.bodyType;

        // set up the visual sphere here
        visualSphere = new GameObject(gameObject.name + " visuals");
        visualSphere.transform.parent = transform;

        CelestialBodyMesh meshConstructor = visualSphere.AddComponent<CelestialBodyMesh>();
        meshConstructor.Generate(newSettings.structureSettings, newSettings.colorSettings);

        // set the properties of the body from the settings
        surfaceGravity = newSettings.surfaceGravity;
        radius = newSettings.radius;
        mass = radius * radius * surfaceGravity / Universe.gravitationalConstant;
        axisTilt = newSettings.axisTilt;
        axialSpinSpeed = newSettings.axialSpin;

        Quaternion upRot = Quaternion.AngleAxis(axisTilt, Vector3.forward);
        localUpAxis = upRot * Vector3.up;

        // set up the transform
        transform.localScale = Vector3.one;
        visualSphere.transform.localScale = Vector3.one * radius;
        visualSphere.transform.localRotation = upRot;

        // setup the rigidbody
        rb.mass = mass;

        // each object is given a unique id that is generated through its settings
        ID = newSettings.id;
    }

    public void SetUpBody(GenerationSettings.StarSettings starSettings, bool setPositions = true)
    {
        // this is called when a celestial body is wanting to be set up to be a star

        // add a rigidbody to the body
        if (rb is null) rb = gameObject.AddComponent<Rigidbody>();

        // set the type of body
        bodyType = starSettings.bodyType;

        // set up the visual sphere here
        visualSphere = new GameObject(gameObject.name + " visuals");
        visualSphere.transform.parent = transform;

        //CelestialBodyMesh meshConstructor = visualSphere.AddComponent<CelestialBodyMesh>();
        //meshConstructor.Generate(starSettings.structureSettings, starSettings.colorSettings);

        // set the properties of the body from the settings
        surfaceGravity = starSettings.surfaceGravity;
        radius = starSettings.radius;
        mass = radius * radius * surfaceGravity / Universe.gravitationalConstant;
        axisTilt = starSettings.axisTilt;
        axialSpinSpeed = starSettings.axialSpin;

        Quaternion upRot = Quaternion.AngleAxis(axisTilt, Vector3.forward);
        localUpAxis = upRot * Vector3.up;

        StarMesh sm = visualSphere.AddComponent<StarMesh>();
        sm.Setup(starSettings.temperature);

        // set up the transform
        transform.localScale = Vector3.one;
        visualSphere.transform.localScale = Vector3.one * radius * 2;
        visualSphere.transform.localRotation = upRot;

        // setup the rigidbody
        rb.mass = mass;

        // each object is given a unique id that is generated through its settings
        ID = starSettings.id;

        if (setPositions)
        {
            // set up the initial velocity
            // for a star in a single star system, we dont need initial velocity
            // as the star is not moving initially relative to all other objects
            // in the system
            initialVelocity = Vector3.zero;
            rb.position = Vector3.zero;
        }
    }

    public void SetUpBody(GenerationSettings.PlanetSettings planetSettings, bool setPositions = true, CelestialBody newOrbitalParent = null)
    {
        // this is called when a celestial body is wanted to be set up as a planet

        GenericSetup(planetSettings);

        drawPath = planetSettings.drawPath;
        pathLength = planetSettings.pathLength;

        if (setPositions)
        {
            // place the planet away from the star
            Vector3 direction = new Vector3(Mathf.Cos(planetSettings.offsetAngle * Mathf.Deg2Rad), 0, Mathf.Sin(planetSettings.offsetAngle * Mathf.Deg2Rad));
            transform.position = newOrbitalParent.transform.position + (direction * planetSettings.distance);
        }

        // set up the initial velocity
        // so that the planet will fall into a stable orbit around its parent
        // we do this even if we don't want to reset the position to calculate the effect
        // of any changes to the orbit eccentricity
        if (newOrbitalParent != null)
        {
            initialVelocity = planetSettings.eccentricity * CalculateInitialVelocity(newOrbitalParent.transform);
            orbitalParent = newOrbitalParent;
            orbitalParentMass = orbitalParent.GetComponent<CelestialBody>().mass;
        }
    }

    public void SetUpBody(GenerationSettings.MoonSettings moonSettings, bool setPositions = true, CelestialBody newOrbitalParent = null)
    {
        // this is called when a celestial body is wanted to be set up as moon

        GenericSetup(moonSettings);

        drawPath = moonSettings.drawPath;
        pathLength = moonSettings.pathLength;

        if (setPositions)
        {
            // place the moon away from the planet
            Vector3 direction = new Vector3(Mathf.Cos(moonSettings.offsetAngle * Mathf.Deg2Rad), 0, Mathf.Sin(moonSettings.offsetAngle * Mathf.Deg2Rad));
            transform.position = newOrbitalParent.transform.position + (direction * moonSettings.distance);
        }

        // set up the initial velocity to give a stable orbit
        if (newOrbitalParent != null)
        {
            initialVelocity = moonSettings.eccentricity * CalculateInitialVelocity(newOrbitalParent.transform);
            orbitalParent = newOrbitalParent;
            orbitalParentMass = orbitalParent.GetComponent<CelestialBody>().mass;
        }
    }

    public Vector3 CalculateInitialVelocity(Transform toOrbit)
    {

        // first of all we need the vector from our position to the body that we will orbit
        Vector3 d = toOrbit.transform.position - transform.position;

        // then find the semi major axis of our elliptical orbit
        float semiMajorAxis = d.x * d.x + d.z * d.z;

        // then we need the gradient of the tangent line that we currently lie on
        // this will define the direction in which our velocity vector faces
        Vector3 vDir;

        // this needs a special case for when the gradient is undefined
        // this will occur when the body and its parent are in line on the z axis
        if (d.z == 0)
        {
            // however, in this case we know the direction of the velocity vector will be parallel to the z axis
            vDir = Vector3.forward;
        } else {
            // this is the gradient of the tangent line
            float gradient = (d.x * - 1) / d.z;

            // this made sense at the time
            float h = Mathf.Sqrt(1 + gradient * gradient);
            vDir = new Vector3(1/h, 0, gradient/h).normalized;
        }
        // then we can find the magnitude of the velocity
        // this is based on the mass of the object that is being orbited,
        // the distance from that object, and the semi major axis of the orbit

        // so we first need to find the mass of what were orbiting
        float parentMass = toOrbit.GetComponent<CelestialBody>().mass;

        float vMagnitude = Mathf.Sqrt(Universe.gravitationalConstant * parentMass * ((2.0f / d.magnitude) - (1.0f / semiMajorAxis)));

        // then finally we can return the initial velocity of the object
        return vMagnitude * vDir;
    }

    public void UpdateVelocity(ref List<CelestialBody> otherBodies, float timeStep)
    {
        // get the acceleration due to all the objects gravitationally influencing this celestial body
        // and add it onto the current velocity
        Vector3 acceleration = CalculateAcceleration(ref otherBodies, transform.position);
        currentVelocity += acceleration * timeStep;

        // also we want to spin the planet on its axis here
        Quaternion rot = Quaternion.AngleAxis(axialSpinSpeed * timeStep, Vector3.up);
        visualSphere.transform.localRotation *= rot;
    }

    private Vector3 CalculateAcceleration(ref List<CelestialBody> otherBodies, Vector3 pointInSpace)
    {
        // multiple approaches can be taken for calculating forces
        // the force influence of each body can be calculated and summed
        // but this will lead to incredibly high complexity and very unstable orbits

        // a simpler approach is to find the objcet in whose sphere of influence this object lies
        // and move according to the gravity of that object

        // or even simpler, assign a parent body in which this body is influenced by and
        // ignore gravitational influences from any other body

        Vector3 acceleration = Vector3.zero;
        if (orbitalParent != null)
        {
            float sqrDist = (orbitalParent.transform.position - pointInSpace).sqrMagnitude;
            Vector3 forceDir = (orbitalParent.transform.position - pointInSpace).normalized;

            Vector3 force = forceDir * Universe.gravitationalConstant * orbitalParentMass / sqrDist;

            acceleration = force;
        }
        else
        {
            foreach (var otherBody in otherBodies)
            {
                if (otherBody != this)
                {
                    float sqrDist = (otherBody.transform.position - pointInSpace).sqrMagnitude;
                    Vector3 forceDir = (otherBody.transform.position - pointInSpace).normalized;

                    Vector3 force = forceDir * Universe.gravitationalConstant * otherBody.mass / sqrDist;

                    acceleration += force;
                }
            }
        }
        return acceleration;
    }

    public void OrbitLookahead(float pathLength, ref List<CelestialBody> otherBodies, float timeStep)
    {
        // this function draws the predicted orbit of this object
        // it is only a guess based on this objects velocity and the current position of other objects
        // it does not take into account the velocity of the other objects so long term prediction isn't very accurate
        // when there are multiple objects interacting

        Vector3 lAVel;
        if (Application.isPlaying)
            lAVel = currentVelocity;
        else
            lAVel = initialVelocity;
        
        Vector3 lAPos = transform.position;
        float currentPathLength = 0;
        while (currentPathLength < pathLength * pathLength)
        {
            Vector3 acceleration = CalculateAcceleration(ref otherBodies, lAPos);
            
            lAVel += acceleration * timeStep;
            lAPos += lAVel * timeStep;
            
            Vector3 move = lAVel * timeStep;
            Gizmos.DrawLine(lAPos, lAPos + move);

            currentPathLength += move.sqrMagnitude;
        }
    }

    public void UpdatePosition(float timeStep)
    {
        transform.localPosition += currentVelocity * timeStep;
    }

    public Vector3 GetVelocity()
    {
        return currentVelocity;
    }

    public void SetVisualSphere(GameObject newVisualSphere)
    {
        visualSphere = newVisualSphere;
    }
}
