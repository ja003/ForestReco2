using System;
using System.Numerics;
using System.Windows.Forms;

namespace ForestReco
{
	public class CUiRangeController
	{
		private CMainForm form;
		
		public CUiRangeController(CMainForm pForm)
		{
			form = pForm;
		}

		private void SetRangeX()
		{
			CParameterSetter.SetParameter(ESettings.rangeXmin, form.trackBarRangeXmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeXmax, form.trackBarRangeXmax.Value);

			SSplitRange range = CParameterSetter.GetSplitRange();
			form.textRangeXmin.Text = range.ToStringXmin();
			form.textRangeXmax.Text = range.ToStringXmax();

			form.labelRangeXval.Text = range.RangeX.ToString("0.0") + " m";
		}

		private void SetRangeY()
		{
			CParameterSetter.SetParameter(ESettings.rangeYmin, form.trackBarRangeYmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeYmax, form.trackBarRangeYmax.Value);

			SSplitRange range = CParameterSetter.GetSplitRange();
			form.textRangeYmin.Text = range.ToStringYmin();
			form.textRangeYmax.Text = range.ToStringYmax();

			form.labelRangeYval.Text = range.RangeY.ToString("0.0") + " m";
		}

		public void UpdateRangeBounds()
		{
			//X
			//range has to match file coordinates
			//in project are used coordinates moved by offset
			Vector3 min = (Vector3)CProjectData.sourceFileHeader.Min_orig;
			Vector3 max = (Vector3)CProjectData.sourceFileHeader.Max_orig;
			form.trackBarRangeXmin.SetRange((int)min.X * 10, (int)max.X * 10);
			form.trackBarRangeXmax.SetRange((int)min.X * 10, (int)max.X * 10);

			int minValue = CParameterSetter.GetIntSettings(ESettings.rangeXmin);
			int maxValue = CParameterSetter.GetIntSettings(ESettings.rangeXmax);

			//if last values were out of slider bounds, update them
			if(minValue >= form.trackBarRangeXmax.Maximum)
				minValue = form.trackBarRangeXmax.Minimum;
			if(maxValue <= form.trackBarRangeXmax.Minimum)
				maxValue = form.trackBarRangeXmax.Maximum;

			TrySetTrackBarValue(form.trackBarRangeXmin, minValue);
			TrySetTrackBarValue(form.trackBarRangeXmax, maxValue);
			//form.trackBarRangeXmin.Value = 
			//	minValue.LimitToRange(form.trackBarRangeXmin.Minimum, form.trackBarRangeXmin.Maximum);
			//form.trackBarRangeXmax.Value = CUtils.LimitToRange(maxValue,
			//	form.trackBarRangeXmax.Minimum, form.trackBarRangeXmax.Maximum);

			SetRangeX();

			//Y
			form.trackBarRangeYmin.SetRange((int)min.Y * 10, (int)max.Y * 10);
			form.trackBarRangeYmax.SetRange((int)min.Y * 10, (int)max.Y * 10);

			minValue = CParameterSetter.GetIntSettings(ESettings.rangeYmin);
			maxValue = CParameterSetter.GetIntSettings(ESettings.rangeYmax);

			//if last values were out of slider bounds, update them
			if(minValue >= form.trackBarRangeYmax.Maximum)
				minValue = form.trackBarRangeYmax.Minimum;
			if(maxValue <= form.trackBarRangeYmax.Minimum)
				maxValue = form.trackBarRangeYmax.Maximum;

			//WARNING: when float is used there is a possible value overflow
			//form.trackBarRangeYmin.Value = CUtils.LimitToRange(minValue,
			//	form.trackBarRangeYmin.Minimum, form.trackBarRangeYmin.Maximum);
			//form.trackBarRangeYmax.Value = CUtils.LimitToRange(maxValue,
			//	form.trackBarRangeYmax.Minimum, form.trackBarRangeYmax.Maximum);
			TrySetTrackBarValue(form.trackBarRangeYmin, minValue);
			TrySetTrackBarValue(form.trackBarRangeYmax, maxValue);

			SetRangeY();
		}

		private void TrySetTrackBarValue(TrackBar pTrackBar, int pValue)
		{
			pTrackBar.Value = pValue.LimitToRange(pTrackBar.Minimum, pTrackBar.Maximum);
		}

		public void trackBarRangeXmax_Scroll()
		{
			SetRangeX();
		}

		public void trackBarRangeXmin_Scroll()
		{
			SetRangeX();
		}

		public void trackBarRangeYmin_Scroll()
		{
			SetRangeY();
		}

		public void trackBarRangeYmax_Scroll()
		{
			SetRangeY();
		}

		/// <summary>
		/// When user sets range in textBox manually, update the value after the focus of
		/// textbox is lost, otherwise it always changes the selection
		/// </summary>
		internal void textRange_LostFocus(TrackBar pTrackBar, ESettings pOppositeRange, string textRangeValue)
		{
			float value = 0;
			if(textRangeValue.Length == 0)
				return;

			bool parsed;


			bool hasSign = textRangeValue[0] == '+' || textRangeValue[0] == '-';
			if(hasSign)
			{
				int sign = textRangeValue[0] == '-' ? -1 : +1;

				parsed = float.TryParse(textRangeValue.Substring(1, textRangeValue.Length - 1), out value);
				if(!parsed)
					return;

				float newValue = CParameterSetter.GetIntSettings(pOppositeRange) / 10 + sign * value;
				TrySetTrackBarValue(pTrackBar, (int)newValue * 10);
			}
			else
			{
				parsed = float.TryParse(textRangeValue, out value);
				if(!parsed)
					return;
				TrySetTrackBarValue(pTrackBar, (int)value * 10);
			}
			SetRangeX();
			SetRangeY();
		}

		internal void comboBoxSplitMode_SelectedIndexChanged(string pCurrentSplitMode)
		{
			ESplitMode mode;
			Enum.TryParse(pCurrentSplitMode, out mode);
			SetSplitMode(mode);
		}

		private void SetSplitMode(ESplitMode pMode)
		{
			CParameterSetter.SetParameter(ESettings.currentSplitMode, (int)pMode);

			bool isManual = pMode == ESplitMode.Manual;
			form.trackBarRangeXmin.Enabled = isManual;
			form.trackBarRangeXmax.Enabled = isManual;
			form.trackBarRangeYmin.Enabled = isManual;
			form.trackBarRangeYmax.Enabled = isManual;

			form.textRangeXmin.Enabled = isManual;
			form.textRangeXmax.Enabled = isManual;
			form.textRangeYmin.Enabled = isManual;
			form.textRangeYmax.Enabled = isManual;

			bool isShapefile = pMode == ESplitMode.Shapefile;
			form.textShapefile.Enabled = isShapefile;
			form.btnShapefile.Enabled = isShapefile;
		}

		
	}
}
