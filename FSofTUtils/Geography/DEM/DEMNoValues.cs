﻿using System;

namespace FSofTUtils.Geography.DEM {

   [Serializable]
   public class DEMNoValues : DEM1x1 {

      public DEMNoValues(int left, int bottom) :
         base(left, bottom) {
      }

      public override void SetDataArray() {
         Minimum = Maximum = DEMNOVALUE;
         Rows = Columns = 2;   // 2, damit Delta noch einen Wert erhält
         data = new short[Rows * Columns];
         for (int i = 0; i < data.Length; i++)
            data[i] = DEMNOVALUE;
         NotValid = data.Length;
      }

      override protected byte fastgetShadingValue4XY(int x, int y) => 255;

      override public byte InterpolatedShadingValue(double lon, double lat, InterpolationType intpol) => 255;

   }
}
