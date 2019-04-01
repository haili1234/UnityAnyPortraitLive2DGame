/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apTransform_Vertex
	{
		// Members
		//--------------------------------------------
		[SerializeField]
		public int _meshUniqueID = -1;

		[SerializeField]
		public int _vertexID = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apVertex _vertex = null;

		[SerializeField]
		public apMatrix _matrix = new apMatrix();


		// Init
		//--------------------------------------------
		public apTransform_Vertex()
		{

		}


		// Functions
		//--------------------------------------------



		// Get / Set
		//--------------------------------------------
	}
}