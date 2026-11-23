using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AutoJumpMod;

internal class ModFlagChecker
{
	[CompilerGenerated]
	private string _003CqualifiedTypeName_003EP;

	[CompilerGenerated]
	private string _003CprivateFieldName_003EP;

	private Type _type;

	private FieldInfo _field;

	private bool _initialized;

	public ModFlagChecker(string qualifiedTypeName, string privateFieldName = "")
	{
		_003CqualifiedTypeName_003EP = qualifiedTypeName;
		_003CprivateFieldName_003EP = privateFieldName;
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			_type = Type.GetType(_003CqualifiedTypeName_003EP);
			if (!(_type == null) && !string.IsNullOrEmpty(_003CprivateFieldName_003EP))
			{
				_field = _type.GetField(_003CprivateFieldName_003EP, BindingFlags.Instance | BindingFlags.NonPublic);
				_ = _field == null;
			}
		}
	}

	private UnityEngine.Object FindInstance()
	{
		Initialize();
		if (_type == null)
		{
			return null;
		}
		MethodInfo methodInfo = typeof(UnityEngine.Object).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo m) => m.Name == "FindObjectOfType" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);
		if (methodInfo == null)
		{
			return null;
		}
		return methodInfo.MakeGenericMethod(_type).Invoke(null, Array.Empty<object>()) as UnityEngine.Object;
	}

	public bool IsLoaded()
	{
		return FindInstance() != null;
	}

	public bool GetBoolFlag()
	{
		UnityEngine.Object @object = FindInstance();
		if (@object == null || _field == null)
		{
			return false;
		}
		return (bool)_field.GetValue(@object);
	}
}
