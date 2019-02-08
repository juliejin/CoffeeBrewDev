using UnityEngine;
using System.Collections.Generic;

public class IngredientsController : MonoBehaviour {

	//***************************************************************************//
	// Main class for Handling all things related to ingredients
	//***************************************************************************//

	//public list of all available ingredients.
	public static GameObject[] ingredientsArray;
	//Public ID of this ingredient. (used to build up the delivery queue based on customers orders)
	public int factoryID;
    public string stockName;
	public AudioClip itemPick;
    public int price;
	//***************************************************************************//
	// Init
	//***************************************************************************//
	void Awake (){
	}

	void Update (){

	}

    //if there is enough stocks
    /*bool decreaseStock(int num) {
        if (stockNum >= num)
        {
            stockNum -= num;
            stockNumText.text = stockName + " x " + stockNum;
            playSfx(itemPick);
            return true;
        }
        return false;
    }*/


    void createIngredient (){
		if(!MainGameController.gameIsFinished && !MainGameController.deliveryQueueIsFull) {
			GameObject prod = Instantiate(ingredientsArray[factoryID - 1], transform.position + new Vector3(0,0, -1), Quaternion.Euler(90, 180, 0)) as GameObject;
			prod.name = ingredientsArray[factoryID - 1].name;
			prod.tag = "deliveryQueueItem";
			prod.GetComponent<MeshCollider>().enabled = false;
			prod.transform.localScale = new Vector3(0.17f, 0.01f, 0.135f);
			playSfx(itemPick);
		}
	}

	//***************************************************************************//
	// Play AudioClips
	//***************************************************************************//
	void playSfx ( AudioClip _sfx  ){
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}

}