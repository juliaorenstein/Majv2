﻿using Fusion;

public interface ICharlestonFusion
{
    int Counter { get; set; }

    void RPC_C2H_StartPass(int[] tileIDsToPass);
    void RPC_H2C_UpdateRack(int playerId, int[] newRack);
}