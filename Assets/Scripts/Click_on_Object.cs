using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_on_Object : MonoBehaviour{

    private void Update() {
        if (Input.GetMouseButtonDown(0)){
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {    
                if (hit.collider.tag == "Piece"){
                    PrintName(hit.transform.gameObject);
                }
            }
        }
    }

    private void PrintName(GameObject go){
        print(go.name);
    }

}
