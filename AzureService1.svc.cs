using System;
using System.Collections.Generic;
using System.Linq;

namespace WCFServiceWebRole1
{
    public class Service1 : IService1
    {       
        public ConfRoomDto AddMotionReading(string SalID, string SalTimeS, string SalSource, int SalMotion)
        {            
            using (var context = new ConfRoomDB10Entities1())
            {
                ConfRoomTable1 MotionEntry = new ConfRoomTable1()
                {
                    ID = SalID,
                    TimeS = SalTimeS,
                    SourceDevice = SalSource,
                    Motion = SalMotion,
                };
                context.ConfRoomTable1.Add(MotionEntry);
                context.SaveChanges();

                return new ConfRoomDto()
                {
                    ID = MotionEntry.ID,
                    TimeS = MotionEntry.TimeS,
                    Source = MotionEntry.SourceDevice,
                    Motion = (int) MotionEntry.Motion,

                };
            }
        }
    }
}
