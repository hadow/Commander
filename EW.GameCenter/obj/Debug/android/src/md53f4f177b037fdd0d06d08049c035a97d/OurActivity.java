package md53f4f177b037fdd0d06d08049c035a97d;


public class OurActivity
	extends md5c1c2ef97892978a43b6c2975a8e71e2f.AndroidGameActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("EW.OurActivity, EW.GameCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", OurActivity.class, __md_methods);
	}


	public OurActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == OurActivity.class)
			mono.android.TypeManager.Activate ("EW.OurActivity, EW.GameCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
