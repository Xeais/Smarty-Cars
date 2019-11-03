using UnityEngine;

public class CarSettings : Singleton<CarSettings>
{
	[Header("Health")]
  [SerializeField] private float startingCarLife = 600f;
  public float StartingCarLife {get => startingCarLife;}
  [SerializeField] private float dieSpeed = 50f;
  public float DieSpeed {get => dieSpeed;}
  [Header("Finish")]
  [SerializeField] private int targetFinishCrossTimes = 3;
  public int TargetFinishCrossTimes {get => targetFinishCrossTimes;}
  [SerializeField] private float finishFitnessMultiplier = 2f;
  public float FinishFitnessMultiplier {get => finishFitnessMultiplier;}
  [SerializeField] private float fitnessMultiplierForBeingFirst = 2f;
  public float FitnessMultiplierForBeingFirst {get => fitnessMultiplierForBeingFirst;}
  [Header("Other Fitness")]
  [SerializeField] private float fitnessPerUnit = 10f;
  public float FitnessPerUnit {get => fitnessPerUnit;}
  [SerializeField] private float maxTimeWithNoFitnessImprovement = 5f;
  public float MaxTimeWithNoFitnessImprovement {get => maxTimeWithNoFitnessImprovement;}

  protected CarSettings() {}
}