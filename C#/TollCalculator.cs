using System;
using System.Globalization;
using TollFeeCalculator;

namespace TollFeeCalculator
{
    public static class TollCalculator
    {

        /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total toll fee for that day
         */

        public static int GetTollFee(IVehicle vehicle, DateTime[] dates)
        {
            if (vehicle.IsTollFree())
                return 0;

            /*  From the requirements:
                  A vehicle should only be charged once an hour
                      - In the case of multiple fees in the same hour period, the highest one applies.
            
                However, this can be interpreted in many different ways.

                To visualize this let's consider a vehicle incurring a fee every 40 minutes throughout the day
                and let's imagine the individual tolls would be the following:
                A: 8
                B: 8
                C: 13
                D: 18
                E: 13
                F: 8

                If we let the time frame extend (as in the TimeFrameVariant.Extending implementation) since only
                40 minutes has passed since the last fee, all fees would be considered to be within the same hour.
                This would result in the vehicle only being charged for D for a total of 18.

                If we instead set a fixed time frame (as in the TimeFrameVariant.Fixed implementation) the vehile
                would be charged for multiple passings. Once the vehicle reaches C more than an hour has passed since
                A so a new fee is incurred. However since there's only 40 minutes to D that higher fee is used for
                this hourly period. The vehicle then incurrs a fee for E as well since more than an hour has passed
                since C where the period started. The vehicle will therefore be charged for A, D and E for a total
                of 39.

                The first approach lets the vehicle skip payment by merging all instances into a single "hour" but
                the second approach technically charged it for two tolls only 40 minutes apart. We can address this
                with another variant (as in the TimeFrameVariant.ExtendingOnHigherFee implementation) where we only
                consider the time since the actual charge. This would incurr payments at A, D and F for a total of
                34.

                While this third approach looks good given this example, it's possible to think up a scenario where
                the resulting fee doesn't seem to match "what one would expect". For instance, if the vehicle
                never appeared at A, we would only charge for D and F. This means B would be skipped despite being 
                80 minutes before the first actual charge. However, charging for B would go against the requirement
                since C is within the hour with a higher fee, but we can't charge for C since it's within 40 minutes
                of D.

                In conclusion, the requirement of "A vehicle should only be charged once an hour" is rather unclear.
            */

            return GetTotalFee(dates, 60, TimeFrameVariant.ExtendingOnHigherFee);
        }

        public static int GetTollFee(DateTime date)
        {
            if (IsTollFreeDate(date)) return 0;

            int hour = date.Hour;
            int minute = date.Minute;

            if (hour == 6 && minute >= 0 && minute <= 29) return 8;
            else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
            else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
            else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
            else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
            else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
            else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
            else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
            else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
            else return 0;
        }

        private static Boolean IsTollFreeDate(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            // TODO: Figure out bank holidays

            return false;
        }


        private enum TimeFrameVariant
        {
            Fixed,
            Extending,
            ExtendingOnHigherFee,
        }

        private static int GetTotalFee(DateTime[] dates, int max, TimeFrameVariant variant)
        {
            int total = 0;
            int prevFee = 0;
            DateTime? prevTime = null;

            foreach (DateTime date in dates)
            {
                int fee = GetTollFee(date);
                if (fee == 0)
                    continue;

                if (!prevTime.HasValue || (date - prevTime.Value).TotalHours > 1)
                {
                    total += fee;
                    prevFee = fee;
                    prevTime = date;
                }
                else if (fee > prevFee)
                {
                    total += fee - prevFee;
                    prevFee = fee;
                    if (variant != TimeFrameVariant.Fixed)
                        prevTime = date;
                }
                else if (variant == TimeFrameVariant.Extending)
                    prevTime = date;


                if (total >= max)
                    return max;
            }

            return total;
        }
    }
}