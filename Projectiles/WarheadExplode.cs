﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeraldFitzTheNPC.Projectiles {
	// to investigate: Projectile.Damage, (8843)
	internal class WarheadExplode : ModProjectile {
		public override void SetDefaults() {
			// while the sprite is actually bigger than 15x15, we use 15x15 since it lets the projectile clip into tiles as it bounces. It looks better.
			projectile.CloneDefaults(ProjectileID.EnchantedBoomerang);

			// Explode Instantly
			projectile.timeLeft = 0;
			projectile.friendly = true;
			projectile.hostile = true;

			// These 2 help the projectile hitbox be centered on the projectile sprite.
			drawOffsetX = 5;
			drawOriginOffsetY = 5;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			// Vanilla explosions do less damage to Eater of Worlds in expert mode, so we will too.
			if (Main.expertMode) {
				if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) {
					damage /= 5;
				}
			}
		}

		public override void Kill(int timeLeft) {
			// Play explosion sound
			Main.PlaySound(mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.Custom, "Sounds/Custom/WarheadSound"), projectile.position);
			// Smoke Dust spawn
			for (int i = 0; i < 50; i++)
			{
				int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
				Main.dust[dustIndex].velocity *= 1.4f;
			}
			// Fire Dust spawn
			for (int i = 0; i < 80; i++)
			{
				int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 3f);
				Main.dust[dustIndex].noGravity = true;
				Main.dust[dustIndex].velocity *= 5f;
				dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
				Main.dust[dustIndex].velocity *= 3f;
			}
			// Large Smoke Gore spawn
			for (int g = 0; g < 2; g++)
			{
				int goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
				goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
			}
			// reset size to normal width and height.
			projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
			projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
			projectile.width = 10;
			projectile.height = 10;
			projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
			projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);

			// TODO, tmodloader helper method
			{
				int explosionRadius = 75;
				int minTileX = (int)(projectile.position.X / 16f - (float)explosionRadius);
				int maxTileX = (int)(projectile.position.X / 16f + (float)explosionRadius);
				int minTileY = (int)(projectile.position.Y / 16f - (float)explosionRadius);
				int maxTileY = (int)(projectile.position.Y / 16f + (float)explosionRadius);

				// fix to not alter blocks outside of the world
				if (minTileX < 0) {
					minTileX = 0;
				}
				if (maxTileX > Main.maxTilesX) {
					maxTileX = Main.maxTilesX;
				}
				if (minTileY < 0) {
					minTileY = 0;
				}
				if (maxTileY > Main.maxTilesY) {
					maxTileY = Main.maxTilesY;
				}
				bool canKillWalls = false;
				for (int x = minTileX; x <= maxTileX; x++)
				{
					for (int y = minTileY; y <= maxTileY; y++)
					{
						float diffX = Math.Abs((float)x - projectile.position.X / 16f);
						float diffY = Math.Abs((float)y - projectile.position.Y / 16f);
						double distance = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
						if (distance < (double)explosionRadius && Main.tile[x, y] != null && Main.tile[x, y].wall == 0)
						{
							canKillWalls = true;
							break;
						}
					}
				}
				AchievementsHelper.CurrentlyMining = true;
				for (int i = minTileX; i <= maxTileX; i++)
				{
					for (int j = minTileY; j <= maxTileY; j++)
					{
						float diffX = Math.Abs((float)i - projectile.position.X / 16f);
						float diffY = Math.Abs((float)j - projectile.position.Y / 16f);
						double distanceToTile = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
						if (distanceToTile < (double)explosionRadius)
						{
							bool canKillTile = true;
							if (Main.tile[i, j] != null && Main.tile[i, j].active())
							{
								canKillTile = true;
								//dungeon blocks
								if (Main.tileDungeon[(int)Main.tile[i, j].type] || Main.tile[i, j].type == 88 || Main.tile[i, j].type == 21 || Main.tile[i, j].type == 26 || Main.tile[i, j].type == 107 || Main.tile[i, j].type == 108 || Main.tile[i, j].type == 111 || Main.tile[i, j].type == 226 || Main.tile[i, j].type == 237 || Main.tile[i, j].type == 221 || Main.tile[i, j].type == 222 || Main.tile[i, j].type == 223 || Main.tile[i, j].type == 211 || Main.tile[i, j].type == 404)
								{
									canKillTile = false;
								}
								if (!Main.hardMode && Main.tile[i, j].type == 58) //hardmode allow destruction of Hellstone
								{
									canKillTile = false;
								}
								if (!TileLoader.CanExplode(i, j)) //verify if tile can fundamentally be blown up or not
								{
									canKillTile = false;
								}
								if (canKillTile)
								{
									WorldGen.KillTile(i, j, false, false, false);
									if (!Main.tile[i, j].active() && Main.netMode != NetmodeID.SinglePlayer)
									{
										NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
									}
								}
							}
							if (canKillTile)
							{
								for (int x = i - 1; x <= i + 1; x++)
								{
									for (int y = j - 1; y <= j + 1; y++)
									{
										if (Main.tile[x, y] != null && Main.tile[x, y].wall > 0 && canKillWalls && WallLoader.CanExplode(x, y, Main.tile[x, y].wall))
										{
											WorldGen.KillWall(x, y, false);
											if (Main.tile[x, y].wall == 0 && Main.netMode != NetmodeID.SinglePlayer)
											{
												NetMessage.SendData(MessageID.TileChange, -1, -1, null, 2, (float)x, (float)y, 0f, 0, 0, 0);
											}
										}
									}
								}
							}
						}
					}
				}
				AchievementsHelper.CurrentlyMining = false;
			}
		}
	}
}