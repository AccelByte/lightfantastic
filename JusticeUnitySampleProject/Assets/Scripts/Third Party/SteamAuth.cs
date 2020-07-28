// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.
#if !DISABLESTEAMWORKS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Text;

public class SteamAuth : MonoBehaviour
{

    #region
    private byte[] m_Ticket;
    private uint m_pcbTicket;
    private HAuthTicket m_HAuthTicket;
    #endregion

    protected Callback<SteamServersConnected_t> m_SteamServersConnected;
    protected Callback<SteamServerConnectFailure_t> m_SteamServerConnectFailure;
    protected Callback<SteamServersDisconnected_t> m_SteamServersDisconnected;
    protected Callback<ClientGameServerDeny_t> m_ClientGameServerDeny;
    protected Callback<IPCFailure_t> m_IPCFailure;
    protected Callback<LicensesUpdated_t> m_LicensesUpdated;
    protected Callback<ValidateAuthTicketResponse_t> m_ValidateAuthTicketResponse;
    protected Callback<MicroTxnAuthorizationResponse_t> m_MicroTxnAuthorizationResponse;
    protected Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;
    protected Callback<GameWebCallback_t> m_GameWebCallback;

    private CallResult<EncryptedAppTicketResponse_t> OnEncryptedAppTicketResponseCallResult;
    private CallResult<StoreAuthURLResponse_t> OnStoreAuthURLResponseCallResult;
    private CallResult<MarketEligibilityResponse_t> OnMarketEligibilityResponseCallResult;
    private CallResult<DurationControl_t> OnDurationControlCallResult;

    public void OnEnable()
    {
        m_SteamServersConnected = Callback<SteamServersConnected_t>.Create(OnSteamServersConnected);
        m_SteamServerConnectFailure = Callback<SteamServerConnectFailure_t>.Create(OnSteamServerConnectFailure);
        m_SteamServersDisconnected = Callback<SteamServersDisconnected_t>.Create(OnSteamServersDisconnected);
        m_ClientGameServerDeny = Callback<ClientGameServerDeny_t>.Create(OnClientGameServerDeny);
        m_IPCFailure = Callback<IPCFailure_t>.Create(OnIPCFailure);
        m_LicensesUpdated = Callback<LicensesUpdated_t>.Create(OnLicensesUpdated);
        m_ValidateAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.Create(OnValidateAuthTicketResponse);
        m_MicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
        m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);
        m_GameWebCallback = Callback<GameWebCallback_t>.Create(OnGameWebCallback);

        OnEncryptedAppTicketResponseCallResult = CallResult<EncryptedAppTicketResponse_t>.Create(OnEncryptedAppTicketResponse);
        OnStoreAuthURLResponseCallResult = CallResult<StoreAuthURLResponse_t>.Create(OnStoreAuthURLResponse);
        OnMarketEligibilityResponseCallResult = CallResult<MarketEligibilityResponse_t>.Create(OnMarketEligibilityResponse);
        OnDurationControlCallResult = CallResult<DurationControl_t>.Create(OnDurationControl);
    }


    // Start is called before the first frame update
    public string GetSteamTicket()
    {
        if (SteamManager.Initialized)
        {
            //Debug.Log("[" + GetAuthSessionTicketResponse_t.k_iCallback + " - GetAuthSessionTicketResponse] - " + pCallback.m_hAuthTicket + " -- " + pCallback.m_eResult);
            m_Ticket = new byte[1024];
            m_HAuthTicket = SteamUser.GetAuthSessionTicket(m_Ticket, 1024, out m_pcbTicket);
            print("SteamUser.GetAuthSessionTicket(Ticket, 1024, out pcbTicket) - " + m_HAuthTicket + " -- " + m_pcbTicket);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < m_pcbTicket; i++)
            {
                sb.AppendFormat("{0:x2}", m_Ticket[i]);
            }
            string hexTicket = sb.ToString();


            Debug.Log(hexTicket);
            return hexTicket;
        }
        else
        {
            return "00000000";
        }
    }

    void OnSteamServersConnected(SteamServersConnected_t pCallback)
    {
        Debug.Log("[" + SteamServersConnected_t.k_iCallback + " - SteamServersConnected]");
    }

    void OnSteamServerConnectFailure(SteamServerConnectFailure_t pCallback)
    {
        Debug.Log("[" + SteamServerConnectFailure_t.k_iCallback + " - SteamServerConnectFailure] - " + pCallback.m_eResult + " -- " + pCallback.m_bStillRetrying);
    }

    void OnSteamServersDisconnected(SteamServersDisconnected_t pCallback)
    {
        Debug.Log("[" + SteamServersDisconnected_t.k_iCallback + " - SteamServersDisconnected] - " + pCallback.m_eResult);
    }

    void OnClientGameServerDeny(ClientGameServerDeny_t pCallback)
    {
        Debug.Log("[" + ClientGameServerDeny_t.k_iCallback + " - ClientGameServerDeny] - " + pCallback.m_uAppID + " -- " + pCallback.m_unGameServerIP + " -- " + pCallback.m_usGameServerPort + " -- " + pCallback.m_bSecure + " -- " + pCallback.m_uReason);
    }

    void OnIPCFailure(IPCFailure_t pCallback)
    {
        Debug.Log("[" + IPCFailure_t.k_iCallback + " - IPCFailure] - " + pCallback.m_eFailureType);
    }

    void OnLicensesUpdated(LicensesUpdated_t pCallback)
    {
        Debug.Log("[" + LicensesUpdated_t.k_iCallback + " - LicensesUpdated]");
    }

    void OnValidateAuthTicketResponse(ValidateAuthTicketResponse_t pCallback)
    {
        Debug.Log("[" + ValidateAuthTicketResponse_t.k_iCallback + " - ValidateAuthTicketResponse] - " + pCallback.m_SteamID + " -- " + pCallback.m_eAuthSessionResponse + " -- " + pCallback.m_OwnerSteamID);
    }

    void OnEncryptedAppTicketResponse(EncryptedAppTicketResponse_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + EncryptedAppTicketResponse_t.k_iCallback + " - EncryptedAppTicketResponse] - " + pCallback.m_eResult);

        // This code is taken directly from SteamworksExample/SpaceWar
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Successfully retrieved Encrypted App Ticket");
        }
    }

    void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallback)
    {
        Debug.Log("[" + MicroTxnAuthorizationResponse_t.k_iCallback + " - MicroTxnAuthorizationResponse] - " + pCallback.m_unAppID + " -- " + pCallback.m_ulOrderID + " -- " + pCallback.m_bAuthorized);
    }

    void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
    {
        Debug.Log("[" + GetAuthSessionTicketResponse_t.k_iCallback + " - GetAuthSessionTicketResponse] - " + pCallback.m_hAuthTicket + " -- " + pCallback.m_eResult);
        //m_Ticket = new byte[1024];
        //m_HAuthTicket = SteamUser.GetAuthSessionTicket(m_Ticket, 1024, out m_pcbTicket);
        //print("SteamUser.GetAuthSessionTicket(Ticket, 1024, out pcbTicket) - " + m_HAuthTicket + " -- " + m_pcbTicket);
        //StringBuilder sb = new StringBuilder();
        //foreach (byte b in m_Ticket)
        //{
        //    sb.AppendFormat("{0:x2}", b);
        //}
        //string hexTicket = sb.ToString();
        

        //Debug.Log(hexTicket);

    }

    void OnGameWebCallback(GameWebCallback_t pCallback)
    {
        Debug.Log("[" + GameWebCallback_t.k_iCallback + " - GameWebCallback] - " + pCallback.m_szURL);
    }

    void OnStoreAuthURLResponse(StoreAuthURLResponse_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + StoreAuthURLResponse_t.k_iCallback + " - StoreAuthURLResponse] - " + pCallback.m_szURL);
    }

    void OnMarketEligibilityResponse(MarketEligibilityResponse_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + MarketEligibilityResponse_t.k_iCallback + " - MarketEligibilityResponse] - " + pCallback.m_bAllowed + " -- " + pCallback.m_eNotAllowedReason + " -- " + pCallback.m_rtAllowedAtTime + " -- " + pCallback.m_cdaySteamGuardRequiredDays + " -- " + pCallback.m_cdayNewDeviceCooldown);
    }

    void OnDurationControl(DurationControl_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + DurationControl_t.k_iCallback + " - DurationControl] - " + pCallback.m_eResult + " -- " + pCallback.m_appid + " -- " + pCallback.m_bApplicable + " -- " + pCallback.m_csecsLast5h + " -- " + pCallback.m_progress + " -- " + pCallback.m_notification);
    }

}
#endif