using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ModularWorldGenerator : MonoBehaviour
{
    

    public Module[] Modules;
    public Module StartModule;

    public Text numberText;

    private List<Module> allModules;  //used for storing every module on the map

    public int Iterations = 5;

    public float maxRoomX; //to set range of the room size
    public float minRoomX;
    public float maxRoomY;
    public float minRoomY;



    void Start()
    {
        allModules = new List<Module>();
        Module startModule = (Module)Instantiate(StartModule, transform.position, transform.rotation);

        var pendingExits = new List<ModuleConnector>(startModule.GetExits());
        allModules.Add(startModule);

        for (int iteration = 0; iteration < Iterations; iteration++)
        {
            var newExits = new List<ModuleConnector>();

            if (iteration < Iterations - 1)
            {
                foreach (var pendingExit in pendingExits)
                {
                    var newTag = GetRandom(pendingExit.Tags);//get a random tag from the exit.tags[] list
                    var newModulePrefab = GetRandomWithTag(Modules, newTag);//use the tag to get a module
                    var newModule = (Module)Instantiate(newModulePrefab);

                    if (newModule.Tags[0] == "room") //if the new module is a room
                    {
                        if (!newModule.Tags.Contains("pentagon")) // only randomize scale for normal room
                        {
                            newModule.transform.localScale = new Vector3(Random.Range(minRoomX, maxRoomX), Random.Range(minRoomY, maxRoomY), 1);
                            
                        } //set the size of room to random
                    }

                    var newModuleExits = newModule.GetExits();
                    var exitToMatch = newModuleExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newModuleExits);
                    MatchExits(pendingExit, exitToMatch);
                    newModule.lastExit = pendingExit;   

                    
                    OverlapDetection(ref newModule); //detect if there is an overlap

                    if (newModule.isOverlapping)
                    {
                        newExits.Add(pendingExit);  //throw the current pending exit back to the list
                        Destroy(newModule.gameObject);
                        
                        //pendingExit.SetParentNextIsRoom(false);
                    }
                    else
                    {

                        newExits.AddRange(newModuleExits.Where(e => e != exitToMatch)); // add the other exits to pendingList

                        pendingExit.IsOccupied = true;    //set both new and old exits to be occupied
                        exitToMatch.IsOccupied = true;

                        allModules.Add(newModule);
                        DyeModule(pendingExit, newModule); //check and add color for corridor

                    }


                }



                pendingExits = newExits;
            }
            else if (iteration == Iterations - 1)  //last iteration
            {


                LastIterationRecursion(ref pendingExits, 0);  //recursive function, the second parameter is used to 
                                                              //record the number of current iteration


            }
        }

        foreach(var module in allModules)
        {
            var textToShowNumber = (Text)Instantiate(numberText, module.transform.position, Quaternion.identity);
            textToShowNumber.transform.SetParent(GameObject.Find("Canvas").transform);
            module.noOfOccupiedExits=module.GetNoOfOccupiedExits();
            textToShowNumber.text = module.noOfOccupiedExits.ToString();
        }

    }


    private void MatchExits(ModuleConnector oldExit, ModuleConnector newExit)//I did not change name of the variables, but I am using
                                                                             //right instead of forward
    {
        var newModule = newExit.transform.parent;
        var forwardVectorToMatch = -oldExit.transform.right; //get opposite as old exit's orientation
        var correctiveRotation = Azimuth(forwardVectorToMatch) - Azimuth(newExit.transform.right); //use orientations to calculate angle needed
        newModule.RotateAround(newExit.transform.position, Vector3.forward, correctiveRotation);//rotate
        var correctiveTranslation = oldExit.transform.position - newExit.transform.position;//vector to move
        newModule.transform.position += correctiveTranslation;
    }


    private static TItem GetRandom<TItem>(TItem[] array)
    {
        return array[Random.Range(0, array.Length)];
    }


    private static Module GetRandomWithTag(IEnumerable<Module> modules, string tagToMatch)
    {
        var matchingModules = modules.Where(m => m.Tags.Contains(tagToMatch)).ToArray();
        return GetRandom(matchingModules);
    }


    private static float Azimuth(Vector3 vector)
    {
        return Vector3.Angle(Vector3.right, vector) * Mathf.Sign(vector.y);
    }



    private void DyeModule(ModuleConnector pendingExit, Module newModule)
    {
        if (pendingExit.transform.parent.gameObject.GetComponent<Module>().Tags[0] == "room") // if the module of the current pending exit is a room
        {
            newModule.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);   //set the new module corridor to red
        }
        else if (newModule.gameObject.GetComponent<Module>().Tags[0] == "room") // if the new module is a room
        {
            pendingExit.transform.parent.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f); //set current corridor to red
        }
    }




    private void LastIterationRecursion(ref List<ModuleConnector> pendingExits, int iteration)
    {
        iteration += 1;
        //print(iteration);

        if (pendingExits.Count != 0)
        {
            List<ModuleConnector> unsuccessfulExits = new List<ModuleConnector>(); //used to store all exits that are not done with pruning


            foreach (var pendingExit in pendingExits.Where(e => e.IsDoneWithPruning == false))//only select exits of modules that are not done with pruning
            {


                if (pendingExit.transform.parent.GetComponent<Module>().Tags[0] != "room")//if it is not a room, actually redundant because I have included  every posibility to play safe
                {
                    if (pendingExit.transform.parent.GetComponent<Module>().Tags[0] == "junction")//if the module is a junction
                    {



                        int noOfOccupiedExits = pendingExit.transform.parent.GetComponent<Module>().GetNoOfOccupiedExits();//see how many exits are occupied



                        if (noOfOccupiedExits < 2)   //if occupied less than two, which means the junction is not connected to somewhere else

                        {
                            
                            var moduleExits = pendingExit.transform.parent.GetComponent<Module>().GetExits(); //get all exits of the junction
                            foreach (var moduleExit in moduleExits)
                            {
                                moduleExit.IsDoneWithPruning = true;  //set them to be done with pruning
                            }

                            pendingExit.transform.parent.GetComponent<Module>().lastExit.IsOccupied = false; //set previous module's exit to unOccupied 
                                                                                                             //because the junction is to be destroyed, redundant for the assignment, but keeps the script constant
                            unsuccessfulExits.Add(pendingExit.transform.parent.GetComponent<Module>().lastExit);//add the previous module's exit to unsuccessful list
                            allModules.Remove(pendingExit.transform.parent.GetComponent<Module>()); //remove current module from the allModule list
                            Destroy(pendingExit.transform.parent.gameObject); //destroy current module


                        }

                    }

                    else if (pendingExit.transform.parent.GetComponent<Module>().Tags[0] == "corner") //if is a corner
                    {
                        pendingExit.transform.parent.GetComponent<Module>().lastExit.IsOccupied = false; //same as above
                        unsuccessfulExits.Add(pendingExit.transform.parent.GetComponent<Module>().lastExit);  
                        allModules.Remove(pendingExit.transform.parent.GetComponent<Module>());
                        Destroy(pendingExit.transform.parent.gameObject);
                    }
                    else if (pendingExit.transform.parent.GetComponent<Module>().Tags[0] == "corridor")// if it is a corridor
                    {


                        var newModule = (Module)Instantiate(Modules[0]); //try to instantiate a room
                        newModule.transform.localScale = new Vector3(Random.Range(minRoomX, maxRoomX), Random.Range(minRoomY, maxRoomY), 1);
                        var newModuleExits = newModule.GetExits();
                        var exitToMatch = newModuleExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newModuleExits);


                        MatchExits(pendingExit, exitToMatch);
                        newModule.lastExit = pendingExit;
                        OverlapDetection(ref newModule);  


                        if (newModule.isOverlapping == true) //if it overlaps
                        {



                            Destroy(newModule.gameObject); //destroy the room
                            pendingExit.transform.parent.GetComponent<Module>().lastExit.IsOccupied = false; //set previous exit to be unoccupied
                            unsuccessfulExits.Add(pendingExit.transform.parent.GetComponent<Module>().lastExit);  //add the exit of the previous module to the list
                            allModules.Remove(pendingExit.transform.parent.GetComponent<Module>()); //remove the corridor from allModule list
                            Destroy(pendingExit.transform.parent.gameObject);//destroy the corridor


                        }
                        else //if successfully instantiated
                        {


                            exitToMatch.IsOccupied = true;   //set both exits to occupied
                            pendingExit.IsOccupied = true;
                            allModules.Add(newModule);  //add the room to the list
                            DyeModule(pendingExit, newModule);  //add color to corridor

                        }

                    }
                }


                /*else
                {
                    pendingExit.transform.parent.GetComponent<Module>().noOfOccupiedExits=pendingExit.transform.parent.GetComponent<Module>().GetNoOfOccupiedExits();
                    if (pendingExit.transform.parent.GetComponent<Module>().noOfOccupiedExits == 0) { 
                    print("Isolated");
                    allModules.Remove(pendingExit.transform.parent.GetComponent<Module>());
                    Destroy(pendingExit.transform.parent.gameObject); }
                }*/


            }


            pendingExits = unsuccessfulExits;

            LastIterationRecursion(ref pendingExits, iteration);
        }
        
    }

    public void OverlapDetection(ref Module mModule) //mModule is the newly spawned module
    {
        foreach (Module otherModule in allModules)  //loop through all modules on the map
        {

            if (mModule.isOverlapping == false)
            {
                bool ifOverlapping = mModule.gameObject.GetComponent<SpriteRenderer>().bounds.Intersects(otherModule.gameObject.GetComponent<SpriteRenderer>().bounds);//Detect if there is an overlap

                if (otherModule != mModule.lastExit.gameObject.transform.parent.GetComponent<Module>())//only if the compared module is not the module 
                                                                                                        //that spawned the current module 
                                                                                                        //used to prevent wrong overlap detection caused by inaccurate sprite bounds of shapes like triangle
                {
                    mModule.isOverlapping = ifOverlapping;
                }

            }
            else
            {
                break;
            }
        }
    }

    

}
