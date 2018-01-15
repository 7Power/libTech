﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace libTech.Reflection {
	public static class Reflect {
		public static Assembly GetExeAssembly() {
			return Assembly.GetExecutingAssembly();
		}

		public static Assembly LoadAssembly(string FilePath) {
			return Assembly.Load(File.ReadAllBytes(FilePath));
		}

		public static bool Inherits(Type T, Type Base) {
			return Base.IsAssignableFrom(T);
		}

		public static Type[] GetAllTypes(Assembly Asm) {
			return Asm.GetTypes();
		}
	}
}