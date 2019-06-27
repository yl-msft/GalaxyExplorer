using UnityEngine;

public class ForceSolverFocusManager : MonoBehaviour
{
    public bool IsManipulatingPlanet { get; private set; } = false;

    private ForceSolver[] _planetForceSolvers;
    private ForceSolver _currentlyActiveSolver;

    private void Awake()
    {
        _planetForceSolvers = GetComponentsInChildren<PlanetForceSolver>();
    }

    private void Start()
    {
        foreach (var forceSolver in _planetForceSolvers)
        {
            forceSolver.SetToAttract.AddListener(OnSolverAttraction);
            forceSolver.SetToRoot.AddListener(OnSolverRoot);
            forceSolver.SetToManipulate.AddListener(OnSolverManipulate);
            forceSolver.SetToFree.AddListener(OnSolverFree);
        }
    }

    public void OnSolverAttraction(ForceSolver solver)
    {
        if (_currentlyActiveSolver == solver)
        {
            return;
        }

        if (_currentlyActiveSolver != null)
        {
            _currentlyActiveSolver.ResetToRoot();
        }

        _currentlyActiveSolver = solver;
        foreach (var planetForceSolver in _planetForceSolvers)
        {
            if (solver == planetForceSolver)
            {
                continue;
            }
            planetForceSolver.ResetToRoot();
            planetForceSolver.EnableForce = false;
        }
    }

    public void OnSolverRoot(ForceSolver solver)
    {
        if (_currentlyActiveSolver == solver)
        {
            _currentlyActiveSolver = null;
        }
    }

    public void OnSolverManipulate(ForceSolver solver)
    {
        IsManipulatingPlanet = true;

        if (solver == _currentlyActiveSolver)
        {
            return;
        }
        OnSolverAttraction(solver);
    }

    public void OnSolverFree(ForceSolver solver)
    {
        IsManipulatingPlanet = false;

        foreach (var planetForceSolver in _planetForceSolvers)
        {
            if (solver == planetForceSolver)
            {
                continue;
            }
            planetForceSolver.EnableForce = true;
        }
    }

    public void ResetAllForceSolvers()
    {
        foreach (var planetForceSolver in _planetForceSolvers)
        {
            planetForceSolver.ResetToRoot();
            planetForceSolver.EnableForce = true;
        }
    }
}