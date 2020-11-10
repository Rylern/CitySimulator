using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;

/*
 * 
 *  Change the sun position
 * 
*/
public class SunRotation : MonoBehaviour
{
    private float speed;
    private double latitude;
    private double longitude;
    private TimeSpan UTC;
    private DateTime dateTime;
    private Light sunLight;

    void Awake()
    {
        sunLight = GetComponent<Light>();
        speed = 0f;
        latitude = 62.472217;
        longitude = 6.235064;
        dateTime = DateTime.Now;
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        UTC = TimeZoneInfo.Local.BaseUtcOffset;
    }

    /*
     * Called at each frame
     * Rotate the sun given the speed
    */
    void Update()
    {
        dateTime = dateTime.AddHours(speed * Time.deltaTime);
        Vector3 angles = new Vector3();
        double alt;
        double azi;
        SunPosition.CalculateSunPosition(dateTime, latitude, longitude, UTC, out azi, out alt);

        angles.x = (float)alt * Mathf.Rad2Deg;
        angles.y = (float)azi * Mathf.Rad2Deg;

        transform.eulerAngles = angles;
        sunLight.intensity = Mathf.InverseLerp(-12, 0, angles.x);
    }

    /*
     * Getters and Setters
    */
    public float GetAzimuth()
    {
        return transform.eulerAngles.y;
    }
    public DateTime GetDateTime()
    {
        return dateTime;
    }
    public double GetLatitude()
    {
        return latitude;
    }
    public double GetLongitude()
    {
        return longitude;
    }
    public float GetUTC()
    {
        return (float) UTC.TotalHours;
    }
    public float[] GetAngles(DateTime datTim)
    {
        double alt;
        double azi;
        SunPosition.CalculateSunPosition(datTim, latitude, longitude, UTC, out azi, out alt);
        return new float[2] { (float)alt, (float)azi };;
    }
    public float GetSpeed()
    {
        return speed;
    }

    public void SetLatitude(double lat)
    {
        latitude = lat;
    }
    public void SetLongitude(double lon)
    {
        longitude = lon;
    }
    public void SetUTC(float utc)
    {
        UTC = new TimeSpan((int) utc, (int) ((utc % 1)*60), 0);
    }
    public void SetDateTime(DateTime datTime)
    {
        dateTime = datTime;
    }
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    public void AddSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }
    public void PauseSpeed()
    {
        speed = 0;
    }
}

public static class SunPosition
    {
        /*! 
         * \brief Calculates the sun light. 
         * 
         * CalcSunPosition calculates the suns "position" based on a 
         * given date and time in local time, latitude and longitude 
         * expressed in decimal degrees. It is based on the method 
         * found here: 
         * http://www.astro.uio.no/~bgranslo/aares/calculate.html 
         * The calculation is only satisfiably correct for dates in 
         * the range March 1 1900 to February 28 2100. 
         * \param dateTime Time and date in local time. 
         * \param latitude Latitude expressed in decimal degrees. 
         * \param longitude Longitude expressed in decimal degrees. 
         */
        public static void CalculateSunPosition(
            DateTime dateTime, double latitude, double longitude, TimeSpan UTC, out double outAzimuth, out double outAltitude)
        {
            // Convert to UTC
            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();     
            foreach (TimeZoneInfo timeZone in timeZones)
            {
                if (timeZone.BaseUtcOffset.Equals(UTC))
                {
                    dateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
                    break;
                }
            }

            // Number of days from J2000.0.  
            double julianDate = 367 * dateTime.Year -
                (int)((7.0 / 4.0) * (dateTime.Year +
                (int)((dateTime.Month + 9.0) / 12.0))) +
                (int)((275.0 * dateTime.Month) / 9.0) +
                dateTime.Day - 730531.5;

            double julianCenturies = julianDate / 36525.0;

            // Sidereal Time  
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

            double siderealTimeUT = siderealTimeHours +
                (366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

            double siderealTime = siderealTimeUT * 15 + longitude;

            // Refine to number of days (fractional) to specific time.  
            julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;

            // Solar Coordinates  
            double meanLongitude = CorrectAngle(Mathf.Deg2Rad *
                (280.466 + 36000.77 * julianCenturies));

            double meanAnomaly = CorrectAngle(Mathf.Deg2Rad *
                (357.529 + 35999.05 * julianCenturies));

            double equationOfCenter = Mathf.Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

            double elipticalLongitude =
                CorrectAngle(meanLongitude + equationOfCenter);

            double obliquity = (23.439 - 0.013 * julianCenturies) * Mathf.Deg2Rad;

            // Right Ascension  
            double rightAscension = Math.Atan2(
                Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
                Math.Cos(elipticalLongitude));

            double declination = Math.Asin(
                Math.Sin(rightAscension) * Math.Sin(obliquity));

            // Horizontal Coordinates  
            double hourAngle = CorrectAngle(siderealTime * Mathf.Deg2Rad) - rightAscension;

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            double altitude = Math.Asin(Math.Sin(latitude * Mathf.Deg2Rad) *
                Math.Sin(declination) + Math.Cos(latitude * Mathf.Deg2Rad) *
                Math.Cos(declination) * Math.Cos(hourAngle));

            // Nominator and denominator for calculating Azimuth  
            // angle. Needed to test which quadrant the angle is in.  
            double aziNom = -Math.Sin(hourAngle);
            double aziDenom =
                Math.Tan(declination) * Math.Cos(latitude * Mathf.Deg2Rad) -
                Math.Sin(latitude * Mathf.Deg2Rad) * Math.Cos(hourAngle);

            double azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0) // In 2nd or 3rd quadrant  
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0) // In 4th quadrant  
            {
                azimuth += 2 * Math.PI;
            }

            outAltitude = altitude;
            outAzimuth = azimuth;
        }

        /*! 
        * \brief Corrects an angle. 
        * 
        * \param angleInRadians An angle expressed in radians. 
        * \return An angle in the range 0 to 2*PI. 
        */
        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians % (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }
    }