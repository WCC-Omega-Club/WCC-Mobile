package md59b15f7978abc4d0b2cebe92668d59436;


public class TestActivity
	extends android.support.v4.app.FragmentActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("WCCMobile.TestActivity, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", TestActivity.class, __md_methods);
	}


	public TestActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == TestActivity.class)
			mono.android.TypeManager.Activate ("WCCMobile.TestActivity, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
