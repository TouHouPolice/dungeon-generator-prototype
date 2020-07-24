using UnityEngine;

public class Module : MonoBehaviour
{
	public string[] Tags;
    public ModuleConnector lastExit;  //store the exit that spawned this module

    public bool isOverlapping=false;

    
    //public int collisions;

    public int noOfOccupiedExits;


    private void Start()
    {
        
    }
    private void Update()
    {
        noOfOccupiedExits = GetNoOfOccupiedExits();
        
    }

    public ModuleConnector[] GetExits()
	{
		return GetComponentsInChildren<ModuleConnector>();
	}

    public int GetNoOfOccupiedExits()
    {
        int noOfOccupiedExits = 0;
        var Exits = GetExits();
        foreach(var Exit in Exits)
        {
            if (Exit.IsOccupied)
            {
                noOfOccupiedExits++;
            }
        }
        return noOfOccupiedExits;
    }

    private void FixedUpdate()
    {
        
    }

    /*private void OnTriggerEnter2D(Collider2D other)
    {
        collisions += 1;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        collisions -= 1;
    }*/
   /* public void ProperlyDestroyMyself()
    {

        transform.Translate(Vector3.up * 10000);

        Destroy(gameObject);
    }*/

    
}
