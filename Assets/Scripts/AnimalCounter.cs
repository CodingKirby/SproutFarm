using System.Linq;
using UnityEngine;
using TMPro;

public class AnimalCounter : MonoBehaviour
{
    public TextMeshProUGUI cowCountText;
    public TextMeshProUGUI chickenCountText;

    private Animal[] allAnimals;

    void Start()
    {
        // Find all animals in the scene at the start
        allAnimals = FindObjectsOfType<Animal>();
        UpdateCounts();
    }

    void Update()
    {
        // Optionally, update counts every frame or on some condition
        UpdateCounts();
    }

    void UpdateCounts()
    {


        // Count uncaptured cows
        int uncapturedCows = allAnimals.Count(a => a.gameObject.name.Contains("Cow") && !(a.isFollowing || a.isCaptured));

        // Count uncaptured chickens
        int uncapturedChickens = allAnimals.Count(a => a.gameObject.name.Contains("Chicken") && !(a.isFollowing || a.isCaptured));

        // Update the TextMeshPro UI elements
        cowCountText.text = "" + uncapturedCows;
        chickenCountText.text = "" + uncapturedChickens;
    }
}
