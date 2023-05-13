using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {

        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<int> choosedColor = new NetworkVariable<int>();
        //public NetworkList<int> usedColors;

        public List<Material> materials = new List<Material>();

        public HelloWorldManager helloWorldManager;

        void Awake() {
            //usedColors = new NetworkList<int>();
            helloWorldManager = GameObject.Find("HelloWorldManager").GetComponent<HelloWorldManager>();
        }

        public override void OnNetworkSpawn() 
        {
            if (IsOwner) {
                SubmitInitialPositionRequestServerRpc();
            }
        }

        public void ChangeColor()
        {
            SubmitChangeColorServerRpc();
            //GetComponent<MeshRenderer>().material = materials[choosedColor.Value];
            
        }

        [ServerRpc]
        void SubmitChangeColorServerRpc(){
            
            
            int newColor = -1;  //obligar a entrar en el bucle while
            int oldColor = choosedColor.Value;

            while (newColor < 0)  {
                newColor = Random.Range(0, materials.Count);
                if (helloWorldManager.usedColors.Contains(newColor)) {
                    newColor = -1;
                }
            }

            helloWorldManager.AddColor(newColor);
            helloWorldManager.RemoveColor(oldColor);
            choosedColor.Value = newColor;

            string uc = "";
            foreach(int i in helloWorldManager.usedColors) {
                uc +=  i + " ";
            }
            Debug.Log("UsedColors: " + helloWorldManager.usedColors.Count + ": " + uc);
        }

        [ServerRpc]
        void SubmitInitialPositionRequestServerRpc()
        {
            Position.Value = GetRandomPositionOnPlane();
        }
        
        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        }
        
        void Update()
        {
            GetComponent<MeshRenderer>().material = materials[choosedColor.Value];
            transform.position = Position.Value;
        }

        void Start() {
            if (IsOwner) {
                ChangeColor();
            }
        }
    }
}