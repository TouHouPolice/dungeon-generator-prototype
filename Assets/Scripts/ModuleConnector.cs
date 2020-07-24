using UnityEngine;


public class ModuleConnector : MonoBehaviour
{
	public string[] Tags;
	public bool IsDefault;

    public bool IsOccupied=false;    //record if the module connector is being used
    public bool IsDoneWithPruning = false;   //only used for junctions, if one exit has been looped for pruning, 
                                            //this variable of all other exits of the junction will become true
                                             //to prevent same function being run on other exits again

	void OnDrawGizmos()
	{
		var scale = 1.0f;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * scale);

		Gizmos.color = Color.red;
		//Gizmos.DrawLine(transform.position, transform.position - transform.right * scale);
		Gizmos.DrawLine(transform.position, transform.position + transform.right * scale);

		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + transform.up * scale);
        Gizmos.DrawLine(transform.position, transform.position - transform.up * scale);

        Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 0.125f);
	}

    
}
