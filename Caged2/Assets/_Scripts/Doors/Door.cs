using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

enum DoorDirInfo{

    forw,
    back,
}

public enum DoorStates{

    opened = -1,
    locked,
    closed
}

public class Door : NetworkBehaviour
{

    [SerializeField] DoorDirInfo doorDir;
    [SerializeField] DoorStates doorState;
    Transform door;

    Quaternion closedAngle;
    Quaternion openedAngle;

    [SerializeField] bool open = false;
    [SerializeField] bool close = false;


    [SerializeField] bool openDoor = false;
    [SerializeField] bool closeDoor = false;


    // Start is called before the first frame update
    void Start()
    {
        door = transform;
        closedAngle = door.localRotation;
        openedAngle = doorDir == DoorDirInfo.forw ? new Quaternion(closedAngle.x, 90, closedAngle.z, 1) : new Quaternion(closedAngle.x, -90, closedAngle.z, 1);
    }


    void Update(){

        if(open){

            ChangeState(DoorStates.opened);
            open = false;
        }
        if(close){

            ChangeState(DoorStates.closed);
            close = false;
        }

        if(openDoor){

            door.localRotation = Quaternion.Slerp(door.localRotation, openedAngle, Time.deltaTime);
        }
        if(closeDoor){

            door.localRotation = Quaternion.Slerp(door.localRotation, closedAngle, Time.deltaTime);
        }

        if(door.localRotation == closedAngle && doorState != DoorStates.locked)
            doorState = DoorStates.closed;
        if(door.localRotation == openedAngle)
            doorState = DoorStates.opened;
    }


    public void ChangeState(DoorStates state){

        if(state == DoorStates.opened && doorState != DoorStates.opened){

            openDoor = true;
        }
        if(state == DoorStates.closed && doorState != DoorStates.closed){

            closeDoor = true;
        }
    }
}
