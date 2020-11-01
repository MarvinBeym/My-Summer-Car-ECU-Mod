using MSCLoader;
using System;
using UnityEngine;
using System.Linq;

namespace DonnerTech_ECU_Mod.parts
{
    public class ReplacedPartLogic : MonoBehaviour
    {
        private ReplacedPart part;
        private bool replaceApplied = false;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public bool originalPartInstalled
        {
            get
            {
                return (part.gameObject.transform.parent != null);
            }
        }

        public void Init(ReplacedPart part)
        {
            this.part = part;
        }
    }
}