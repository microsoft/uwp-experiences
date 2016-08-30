using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace Presidents
{
    // TODO: 1.3 - See MainPage.xaml for Ten Foot View Changes 
    // TODO: 1.4 - See the XAML for adding a template for when IsTenFootTrigger fires
    public class IsTenFootTrigger : StateTriggerBase
    {
        public IsTenFootTrigger()
        {
            SetActive(App.IsTenFoot);

            App.TenFootModeChanged += App_TenFootModeChanged;
        }

        private void App_TenFootModeChanged(object sender, EventArgs e)
        {
            SetActive(App.IsTenFoot);
        }
    }
}
