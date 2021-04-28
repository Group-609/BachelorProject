using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FrameData
{
    public string DPG; //difficultyPointGlobal

    public string LPC; //levelProgressionCondition;
    public string DEC; //defeatedEnemiesCountCondition;
    public string SC; //stunCondition;
    public string DRC;  //damageReceivedCondition;

    public string SA;  //spawnAmount;
    public string HR;  //healingRate;
    public string PD;  //paintballDamage;
    public string EMD; //enemyMeleeDamage;
    public string EBD; //enemyBulletDamage;

    public string SM; //spawnMultiplier;
    public string HM; //healingMultiplier;
    public string PDM;//painballDamageMultiplier;
    public string MDM;//meleeDamageMultiplier;
    public string BDM;//bulletDamageMultiplier;

    public string ED; //localPlayerEnemyDefeatsCount;
    public string S;  //localPlayerStuntCount;
    public string TDR; //localPlayerTotalDamageReceived;

    public string time;

    public FrameData(float DPG, float DEC, float SC, float DRC, float PD, float EMD, float EBD, float PDM, float MDM, float BDM, float ED, float S, float TDR, float time)
    {
        this.DPG = DPG.ToString("000.00");
        this.DEC = DEC.ToString("00.00");
        this.SC = SC.ToString("00.00");
        this.DRC = DRC.ToString("00.00");
        this.PD = PD.ToString("00.00");
        this.EMD = EMD.ToString("000.00");
        this.EBD = EBD.ToString("000.00");
        this.PDM = PDM.ToString("00.00");
        this.MDM = MDM.ToString("00.00");
        this.BDM = BDM.ToString("00.00");
        this.ED = ED.ToString("00");
        this.S = S.ToString("00");
        this.TDR = TDR.ToString("0000.00");
        this.time = time.ToString("0000.0");
    }

    public FrameData(float DPG, float LPC, float SA, float HR, float SM, float HM, float time)
    {
        this.DPG = DPG.ToString("000.00");
        this.LPC = LPC.ToString("00.00");
        this.SA = SA.ToString("00");
        this.HR = HR.ToString("00.00");
        this.SM = SM.ToString("00.00");
        this.HM = HM.ToString("00.00");
        this.time = time.ToString("0000.0");
    }
}
