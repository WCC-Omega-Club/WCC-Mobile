package md59b15f7978abc4d0b2cebe92668d59436;


public class ContactsActvity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onBackPressed:()V:GetOnBackPressedHandler\n" +
			"";
		mono.android.Runtime.register ("WCCMobile.ContactsActvity, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ContactsActvity.class, __md_methods);
	}


	public ContactsActvity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ContactsActvity.class)
			mono.android.TypeManager.Activate ("WCCMobile.ContactsActvity, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onBackPressed ()
	{
		n_onBackPressed ();
	}

	private native void n_onBackPressed ();

	java.util.ArrayList refList;
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
