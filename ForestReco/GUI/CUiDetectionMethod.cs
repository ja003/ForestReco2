using System;
using System.Numerics;
using System.Windows.Forms;

namespace ForestReco
{
	/// <summary>
	/// Handles UI of elements related to detection method.
	/// Enables/disables relevant elements to each method.
	/// </summary>
	public class CUiDetectionMethod
	{
		private CMainForm form;
		
		public CUiDetectionMethod(CMainForm pForm)
		{
			form = pForm;
		}

		public void OnSelectDetectMethod(EDetectionMethod pMethod)
		{
			bool detection2D = pMethod == EDetectionMethod.Detection2D;
			SetElementEnabled(form.trackBarLocalMaxHeight, detection2D);
			SetElementEnabled(form.trackBarAllowedDescend, detection2D);
			SetElementEnabled(form.trackBarMinAscendSteps, detection2D);
			SetElementEnabled(form.trackBarMinDescendSteps, detection2D);
			SetElementEnabled(form.richTextTreeRadius, detection2D);
		}

		private void SetElementEnabled(Control pControl, bool pEnabled)
		{
			pControl.Enabled = pEnabled;
			pControl.BackColor = pEnabled ? System.Drawing.Color.White : System.Drawing.Color.Black;
		}
	}
}
