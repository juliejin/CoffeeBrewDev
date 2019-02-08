using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CustomerController : MonoBehaviour {
    
    //***************************************************************************//
    // This class manages all thing related to a customer, including
    // wishlist, wishlist ingredients, patience and animations.
    //***************************************************************************//
    //Modifiable variables (Only Through Inspector - Do not hardcode!)
    public int customerNeeds;                             //Product ID which the customer wants. if left to 0, it randomly chooses a product.
                                                        //set anything but 0 to override it.
                                                        //max limit is the length of the availableProducts array.
    public GameObject positionDummy;                    //dummy to indicate where items should be displayed over customer's head
    public bool showProductIngredientHelpers = true;     //whether to show customer wish's ingredeints as a helper or not (player has to guess)

    // Audio Clips
    public AudioClip orderIsOkSfx;    
    public AudioClip orderIsNotOkSfx;
    public AudioClip receivedSomethingGoodSfx;

    //*** Customer Moods ***//
    //We use different materials for each mood
    /* Currently we have 4 moods: 
     [0]Defalut
     [1]Bored 
     [2]Satisfied
     [3]Angry
     we change to appropriate material whenever needed. 
    */

    //**** Special Variables ******//
    // Warning!!! - Do not Modify //
    public int mySeat;                             //Do not modify this.
    public Vector3 destination;        
    private GameObject gameController;             //Do not modify this.
    private GameObject deliveryPlate;             //Do not modify this.
    public bool  isCloseEnoughToDelivery;         //Do not modify this.
    //****************************//

    //Protected variables. do not edit.
    public GameObject[] availableProducts;        //list of all available product to choose from
    public GameObject[] availableIngredients;    //List of all available ingredients to cook above products
    public GameObject[] helperIngredientDummies;//Position dummies for positioning the helper images for ingredients

    //Private customer variables
    private string customerName;                //random name
    private int productIngredients;                //ingredients of the choosen product
    private int[] productIngredientsIDs;        //IDs of the above ingredients
    private bool  isOnSeat;                        //is customer on his seat?
    private bool  mainOrderIsFulfilled;            //flag to let us know if we have delivered the main order

    //Patience bar GUI items and vars    
    internal float leaveTime;                    
    private float creationTime;                
    private bool  isLeaving;                
    public GameObject requestBubble;            //reference to (gameObject) bubble over customers head
    public GameObject money3dText;                //3d text mesh over customers head after successful delivery

    //Transforms
    private Vector3 startingPosition;            //reference
    bool served;

    void Awake (){
        requestBubble.SetActive(false);
        isCloseEnoughToDelivery = false;
        mainOrderIsFulfilled = false;
        
        isOnSeat = false;
        leaveTime = 0;
        isLeaving = false;
        creationTime = Time.time;
        startingPosition = transform.position;
        gameController = GameObject.FindGameObjectWithTag("GameController");
        deliveryPlate = GameObject.FindGameObjectWithTag("coffeeMaker");

        Init();
        StartCoroutine(goToSeat());
    }


    //***************************************************************************//
    // Here we will initialize all customer related variables.
    //***************************************************************************//
    private GameObject productImage;
    private GameObject[] helperIngredients;
    private GameObject sideReq;
    void Init (){
        //we give this customer a nice name
        served = false;
        customerName = "Customer_" + Random.Range(100, 10000);
        gameObject.name = customerName;

        //choose a product for this customer
        if(customerNeeds == 0){
            //for freeplay mode, customers can choose any product. But in career mode,
            //they have to choose from products we allowed in CareerLevelSetup class.
            /*if(PlayerPrefs.GetString("gameMode") == "CAREER") {
                int totalAvailableProducts = PlayerPrefs.GetInt("availableProducts");
                customerNeeds = PlayerPrefs.GetInt( "careerProduct_" + Random.Range(0, totalAvailableProducts).ToString() );
                customerNeeds -= 1; //Important. We count the indexes from zero, while selecting the products from 1.
                                    //se we subtract a unit from customerNeeds to be equal to main AvailableProducts array.
            } else {*/
                customerNeeds = Random.Range(0, availableProducts.Length);
            //}
        }

        //get and show product's image
        productImage = Instantiate(availableProducts[customerNeeds], positionDummy.transform.position, Quaternion.Euler(90, 180, 0)) as GameObject;
        productImage.name = "customerNeeds";
        productImage.transform.localScale = new Vector3(0.18f, 0.1f, 0.13f);
        productImage.transform.parent = requestBubble.transform;
        

        productIngredients = availableProducts[customerNeeds].GetComponent<ProductManager>().totalIngredients;
        //print(availableProducts[customerNeeds].name + " has " + productIngredients + " ingredients.");
        productIngredientsIDs = new int[productIngredients];
        for(int i = 0; i < productIngredients; i++) {
            productIngredientsIDs[i] = availableProducts[customerNeeds].GetComponent<ProductManager>().ingredientsIDs[i];
            //print(availableProducts[customerNeeds].name + " ingredients ID[" + i + "] is: " + productIngredientsIDs[i]);
        }
        
        //show ingredients in the bubble if needed by developer (can make the game easy or hard)
        helperIngredients = new GameObject[productIngredients];
        for(int j = 0; j < productIngredients; j++) {
            int ingredientID = productIngredientsIDs[j] - 1; // Array always starts from 0, while we named our ingredients from 1.
            helperIngredients[j] = Instantiate(availableIngredients[ingredientID], helperIngredientDummies[j].transform.position, Quaternion.Euler(90, 180, 0)) as GameObject;
            //helperIngredients[j].tag = "ingIcon";
            helperIngredients[j].name = "helperIngredient #" + j;
            helperIngredients[j].transform.parent = requestBubble.transform;
            
            if(!showProductIngredientHelpers) {
                helperIngredients[j].GetComponent<Renderer>().enabled = false;
            }
        }
    }


    //***************************************************************************//
    // After this customer has been instantiated by MainGameController,
    // it starts somewhere outside game scene and then go to it's position (seat)
    // with a nice animation. Then asks for it's order.
    //***************************************************************************//
    private float speed = 3.0f;
    private float timeVariance;
    IEnumerator goToSeat (){
        timeVariance = Random.value;
        while(!isOnSeat) {
            transform.position = new Vector3(transform.position.x + (Time.deltaTime * speed),
                                             startingPosition.y - 0.25f + (Mathf.Sin((Time.time + timeVariance) * 10) / 8),
                                             transform.position.z);
                
            if(transform.position.x >= destination.x) {
                isOnSeat = true;
                requestBubble.SetActive(true);
                yield break;
            }
            yield return 0;
        }
    }

    
    //***************************************************************************//
    // FSM
    //***************************************************************************//
    void Update (){

    }


    //***************************************************************************//
    // We need these events to be checked at the eend of every frame.
    //***************************************************************************//
    void LateUpdate (){
        /*checkDistanceToDelivery();
        //check the status of customers with both main order and side-request, and look if they already received both their orders.
        if(mainOrderIsFulfilled && !isLeaving) {        
            settle();
        }*/
    }

    //***************************************************************************//
    // check distance to delivery
    // we check if player is dragging the order towards the customer or towards it's
    // requestBubble. the if the order was close enough to any of these objects,
    // we se the deliver flag to true.
    //***************************************************************************//
    private float distanceToDelivery;
    private float frameDistanceToDelivery;
    void checkDistanceToDelivery (){
        distanceToDelivery = Vector3.Distance(transform.position, deliveryPlate.transform.position);
        frameDistanceToDelivery = Vector3.Distance(requestBubble.transform.position, deliveryPlate.transform.position);
        //print(gameObject.name + " distance to candy is: " + distanceToDelivery + ".");
        
        //Hardcoded integer for distance.
        if(distanceToDelivery < 1.0f || frameDistanceToDelivery < 1.0f) {
            isCloseEnoughToDelivery = true;
            //tint color
            GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
        } else {
            isCloseEnoughToDelivery = false;
            //reset tint color
            GetComponent<Renderer>().material.color = new Color(1, 1, 1);
        }
    }

    //***************************************************************************//
    // receive and check order contents
    //***************************************************************************//
    public void fulfillOrder() {
        //check if stock enough and let customer leave
        //if enough stock. animate cup
        if (!served && isOnSeat)
        {
            served = true;
            Dictionary<int, int> inv = new Dictionary<int, int>();
            foreach (var item in FindObjectOfType<MainGameController>().inventory){
                inv.Add(item.Key, item.Value);
            }
            bool canServe = true;
            foreach (int id in productIngredientsIDs)
            {
                if (inv.ContainsKey(id))
                {
                    if (inv[id] <= 0)
                    {
                        canServe = false;
                    }
                    else
                    {
                        inv[id] -= 1;
                    }
                }
                else canServe = false;
            }
            mainOrderIsFulfilled = true;
            if (canServe)
            {
                FindObjectOfType<MainGameController>().decreaseInventory(productIngredientsIDs);
                //FindObjectOfType<CoffeeMakerController>().AnimateCup(this.gameObject.transform.position);
                settle();
            }
            else
            {
                StartCoroutine(leave());
            }
        }
    }



    //***************************************************************************//
    // Customer should pay and leave the restaurant.
    //***************************************************************************//
    void settle (){
        //give cash, money, bonus, etc, here.
        float leaveTime = Time.time;

        //if we have purchased additional items for our restaurant, we should receive more tips
        int tips = 0;
        if(PlayerPrefs.GetInt("shopItem-1") == 1) tips += 2;    //if we have seats
        if(PlayerPrefs.GetInt("shopItem-2") == 1) tips += 5;    //if we have music player
        if(PlayerPrefs.GetInt("shopItem-3") == 1) tips += 8;    //if we have flowers
        
        int finalMoney = availableProducts[customerNeeds].GetComponent<ProductManager>().price +  tips;    
        
        MainGameController.totalMoneyMade += finalMoney;
        GameObject money3d = Instantiate(    money3dText, 
                                            transform.position + new Vector3(0, 0, -0.8f), 
                                            Quaternion.Euler(0, 0, 0)) as GameObject;
        print ("FinalMoney: " + finalMoney);
        money3d.GetComponent<TextMeshController>().myText = "$" + finalMoney.ToString();

        playSfx(orderIsOkSfx);
        StartCoroutine(leave());
    }


    //***************************************************************************//
    // Leave routine with get/set ers and animations.
    //***************************************************************************//
    public IEnumerator leave (){
        
        //prevent double animation
        if(isLeaving)
            yield break;

        //set the leave flag to prevent multiple calls to this function
        isLeaving = true;
        MainGameController.totalCustomersServed += 1;
        //animate (close) request bubble
        StartCoroutine(animate(Time.time, requestBubble, 0.75f, 0.95f));
        yield return new WaitForSeconds(0.4f);
        
        //animate mainObject (hide it, then destroy it)
        while(transform.position.x < 10) {
            transform.position = new Vector3(transform.position.x + (Time.deltaTime * speed),
                                             startingPosition.y - 0.25f + (Mathf.Sin(Time.time * 10) / 8),
                                             transform.position.z);
            
            if(transform.position.x >= 10) {
                gameController.GetComponent<MainGameController>().availableSeatForCustomers[mySeat] = true;
                Destroy(gameObject);
                yield break;
            }
            yield return 0;
        }
    }


    //***************************************************************************//
    // animate customer.
    //***************************************************************************//
    IEnumerator animate ( float _time ,   GameObject _go ,   float _in ,   float _out  ){
        float t = 0.0f; 
        while (t <= 1.0f) {
            t += Time.deltaTime * 10;
            _go.transform.localScale = new Vector3(Mathf.SmoothStep(_in, _out, t),
                                                   _go.transform.localScale.y,
                                                   _go.transform.localScale.z);
            yield return 0;
        }
        float r = 0.0f; 
        if(_go.transform.localScale.x >= _out) {
            while (r <= 1.0f) {
                r += Time.deltaTime * 2;
                _go.transform.localScale = new Vector3(Mathf.SmoothStep(_out, 0.01f, r),
                                                       _go.transform.localScale.y,
                                                       _go.transform.localScale.z);
                if(_go.transform.localScale.x <= 0.01f)
                    _go.SetActive(false);
                yield return 0;
            }
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