using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace QtmCat
{
	public class MyImage : Image
	{
		[HideInInspector]
		public PolygonCollider2D polyCollider;

		public string            dirPrefabName;
		public string            spritePrefabName;


		protected override void Start()
		{
			base.Start();
			this.polyCollider = this.GetComponent<PolygonCollider2D>();

			if (this.sprite == null && this.dirPrefabName != "" && this.spritePrefabName != "" && this.dirPrefabName != null && this.spritePrefabName != null)
			{
				//ADebug.Assert(this.dirPrefabName    != "", "Why dirPrefabName    not set");
				//ADebug.Assert(this.spritePrefabName != "", "Why spritePrefabName not set");

				Sprite sprite = AResource.LoadSpriteFromPrefab(this.dirPrefabName, this.spritePrefabName);
				ADebug.Assert(sprite != null, "Can not find SpritePrefab [{0}, {1}]", this.dirPrefabName, this.spritePrefabName);
				this.sprite   = sprite;
			}
		}

		public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (this.polyCollider == null)
			{
				return base.IsRaycastLocationValid(sp, eventCamera);
			}
			else 
			{
				Vector3 world = eventCamera.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 0));
				return this.polyCollider.bounds.Contains(world);
			}
		}
	}

}
