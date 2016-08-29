using System.IO;
using UnityEngine;

namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class MonthlyBudgets : MonoBehaviour
    {
        private double LastUpdate = -648000.0f;
        int BudgetInterval = 64800;
        int friendlyInterval = 30;
        private float Rep = 0.0f;
        private float budget = 0.0f;
        private double funds = 0.0f;
        int multiplier = 2500;
        string SavedFile = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/MonthlyBudget.cfg";

        private void Budget()
        {
            try
            {
                double time = (Planetarium.GetUniversalTime());

                if ((time - LastUpdate) >= BudgetInterval)
                {
                    Rep = Reputation.CurrentRep;

                    budget = Rep * multiplier;
                    if (budget <= multiplier)
                    {
                        budget = multiplier;
                    }
                    funds = Funding.Instance.Funds;
                    Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                    Funding.Instance.AddFunds(budget, TransactionReasons.None);
                    LastUpdate = LastUpdate + BudgetInterval;
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                    }
                }
            }
            catch
            {
            }
        }
            
        void Awake()
        {
                Load();
                
        }

        void Start()
        {
           Budget();
        }
        void Update()
        {
                Budget();
        }
        void OnDestroy()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                Save();
            }
        }
        private void Load()
        {
            if (File.Exists(SavedFile))
            {

                ConfigNode saved = ConfigNode.Load(SavedFile);
                if (saved != null)
                {
                    double.TryParse(saved.GetValue("TimeElapsed"), out LastUpdate);
                    int.TryParse(saved.GetValue("Multiplier"), out multiplier);
                    int.TryParse(saved.GetValue("Budget Interval (Kerbin Days)"), out friendlyInterval);
                    Debug.Log("MonthlyBudgets: Loaded settings");
                    if (friendlyInterval != 30)
                    {
                        BudgetInterval = friendlyInterval * 60 * 60 * 6;
                    }
                }
                else
                {
                    Debug.Log("MonthlyBudgets: No save data found (this message is harmless, as long as this isn't an existing career");
                }
            }
        }
        private void Save()
        {
            ConfigNode savedNode = new ConfigNode("BudgetSave");
            savedNode.AddValue("TimeElapsed", LastUpdate);
            savedNode.AddValue("Multiplier", multiplier);
            savedNode.AddValue("Budget Inverval (Kerbin Days)", friendlyInterval);
            savedNode.Save(SavedFile);
            Debug.Log("MonthlyBudgets: Saved game");
        }
    }
    }
