using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {

        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<int> choosedColor = new NetworkVariable<int>();  // cor actual
        
        public List<Material> materials = new List<Material>();

        public HelloWorldManager helloWorldManager;

        void Awake() {
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
        }

        [ServerRpc]
        void SubmitChangeColorServerRpc(){
            
            int newColor = -1;  //obrigar a entrar no bucle while
            int oldColor = choosedColor.Value;

            // Escollemos unha cor libre aleatoriamente
            while (newColor < 0)  {
                newColor = Random.Range(0, materials.Count);
                if (helloWorldManager.usedColors.Contains(newColor)) {
                    newColor = -1;
                }
            }

            helloWorldManager.AddColor(newColor);
            UnListServerRpc(oldColor);
            choosedColor.Value = newColor;

            string uc = "";
            foreach(int i in helloWorldManager.usedColors) {
                uc +=  i + " ";
            }
            Debug.Log("UsedColors: " + helloWorldManager.usedColors.Count + ": " + uc);
        }

        [ServerRpc]
        void UnListServerRpc(int color) {
            helloWorldManager.RemoveColor(color);
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
                // Evitar que elimine a cor 0 (cor anterior) se, por casualidade,
                // fora a elixida aleatoriamente ao spanearse
                choosedColor.Value = -1;
                ChangeColor();
            }
        }
    }
}