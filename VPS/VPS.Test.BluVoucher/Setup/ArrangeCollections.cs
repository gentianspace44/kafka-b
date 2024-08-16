using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.BluVoucher.Responses;
using VPS.Domain.Models.Common.Request;

namespace VPS.Test.BluVoucher.Setup
{
    public class ArrangeCollections
    {
        public static BluVoucherProviderResponse CreateBluVoucherRemitResponse()
        {
            return new BluVoucherProviderResponse()
            {
                ErrorCode = 1,
                Message = "Voucher redeem successfully",
                Success = true,
                VoucherAmount = 500,
                VoucherID = 1,
            };
        }

        public static BluVoucherRedeemRequest CreateBluVoucherRedeemRequest(Parameters parameters)
        {
            return new BluVoucherRedeemRequest()
            {
                ClientId = parameters.ClientId,
                DevicePlatform = parameters.DevicePlatform,
                VoucherNumber = parameters.VoucherNumber,
                VoucherReference = ""
            };
        }

        public static Domain.Models.BluVoucher.BluVoucher CreateBluVoucher()
        {
            return new Domain.Models.BluVoucher.BluVoucher()
            {
                Amount = 1,
                BonusAmount = 1,
                BonusApplied = true,
                ClientId = 1,
                RedeemDateTime = DateTime.Now,
                SyXPlatform = "mob",
                VoucherPin = "12345678",
                VoucherReference = "123456",
                VoucherReferenceId = 1,
                VoucherStatusId = 1,
                VoucherTransactionTypeId = 1,
                VoucherTypeId = Domain.Models.Enums.VoucherType.BluVoucher
            };
        }

        public static BluLabelProviderRequest CreateBluLabelRequestRedeemVoucher(Parameters para)
        {
            return new BluLabelProviderRequest()
            {
                EventType = "redeemVoucher",
                SessionId = para.SessionId,
                Event = new AirtimeRequestEvent()
                {
                    Reference = para.VoucherReference,
                    Amount = para.Amount,
                    Pin = para.VoucherNumber
                }
            };
        }

        public static BluLabelProviderRequest CreateBluLabelGetVoucherStatus(Parameters para)
        {
            return new BluLabelProviderRequest()
            {
                EventType = "getVoucherStatus",
                SessionId = para.SessionId,
                Event = new AirtimeRequestEvent()
                {
                    Reference = para.VoucherReference,
                    Amount = para.Amount,
                    Pin = para.VoucherNumber
                }
            };
        }

        public static AirtimeAuthenticationResponse CreateAirtimeAuthenticationResponse(Parameters para)
        {
            return new AirtimeAuthenticationResponse()
            {
                Data = new AuthenticationData()
                {
                    Reference = para.VoucherReference,
                    TransTypes = new AuthenticationTransTypes()
                    {
                        TransType = new List<string>() { "", "" }
                    }
                },
                SessionId = para.SessionId,
                Event = new AuthenticationEvent()
                {
                    EventCode = "1"
                },
                EventType = ""
            };
        }

        public static NetworkStream CreateNetworkStream()
        {
            using var client = new TcpClient();            
            client.Connect("example.com", 80);
            return client.GetStream();           
        }

        public static string CreateSuccessStreamResultGetVoucherStatus()
        {
            return "<response><SessionId>9e623d98-b954-4291-b4cd-2edcc3f785e3</SessionId><EventType>redeemVoucher</EventType><event><EventCode>0</EventCode></event><data><Status><Code>2</Code><Description>Voucher redeemed successfully</Description><Amount>10</Amount><RedemtionTransRef>3797439382</RedemtionTransRef><RedemtionDate>2023-09-11 15:05:42</RedemtionDate></Status><Reference>b51b8d1d-ad2b-4c85-aa87-3e074268c4aa</Reference></data></response>";
        }

        public static string CreateSuccessStreamResultRedeemVoucher()
        {
            return "<response><SessionId>9e623d98-b954-4291-b4cd-2edcc3f785e3</SessionId><EventType>redeemVoucher</EventType><event><EventCode>0</EventCode></event><data><Status><Code>0</Code><Description>Voucher redeemed successfully</Description><Amount>10</Amount><RedemtionTransRef>3797439382</RedemtionTransRef><RedemtionDate>2023-09-11 15:05:42</RedemtionDate></Status><Reference>b51b8d1d-ad2b-4c85-aa87-3e074268c4aa</Reference></data></response>";
        }

        public static string CreateFailedStreamResult()
        {
            return "<response><SessionId>9e623d98-b954-4291-b4cd-2edcc3f785e3</SessionId><EventType>redeemVoucher</EventType><event><EventCode>-1</EventCode></event><data><Status><Code>-1</Code><Description>Voucher redeemtion Failed</Description><Amount>10</Amount><RedemtionTransRef>3797439382</RedemtionTransRef><RedemtionDate>2023-09-11 15:05:42</RedemtionDate></Status><Reference>b51b8d1d-ad2b-4c85-aa87-3e074268c4aa</Reference></data></response>";
        }
    }
}
