
using System.ServiceModel;
using System.Collections.Generic;
using System;

namespace WCFServiceWebRole1
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]       
        ConfRoomDto AddMotionReading(string SalID, string SalTimeS, string SalSource, int SalMotion);
    }
}


