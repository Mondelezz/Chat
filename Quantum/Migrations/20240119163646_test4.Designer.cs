﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Quantum.Data;

#nullable disable

namespace Quantum.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20240119163646_test4")]
    partial class test4
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Quantum.GroupFolder.Models.Group", b =>
                {
                    b.Property<Guid>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CountMembers")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DescriptionGroup")
                        .HasColumnType("text");

                    b.Property<Guid>("GroupRequestId")
                        .HasColumnType("uuid");

                    b.Property<string>("LinkInvitation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NameGroup")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("StatusAccess")
                        .HasColumnType("boolean");

                    b.HasKey("GroupId");

                    b.HasIndex("GroupRequestId")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.GroupRequest", b =>
                {
                    b.Property<Guid>("GroupRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CountRequests")
                        .HasColumnType("integer");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid");

                    b.HasKey("GroupRequestId");

                    b.ToTable("GroupRequests");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.GroupRequestUserInfoOutput", b =>
                {
                    b.Property<Guid>("GroupRequestId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserInfoOutputId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("GroupRequestId", "UserInfoOutputId");

                    b.HasIndex("UserInfoOutputId");

                    b.ToTable("GroupRequestUserInfoOutput");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.GroupUserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("JoinDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "GroupId");

                    b.ToTable("GroupUserRole");
                });

            modelBuilder.Entity("Quantum.Models.DTO.TextMessage", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("MessageId"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ReceiverPhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ReceiverUserId")
                        .HasColumnType("uuid");

                    b.Property<string>("SenderPhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SenderUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("SentTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("MessageId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Quantum.UserP.Models.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("HashPassword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Quantum.UserP.Models.UserGroups", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("UserGroups");
                });

            modelBuilder.Entity("Quantum.UserP.Models.UserInfoOutput", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("OpenUsers");
                });

            modelBuilder.Entity("UserFriends", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("FriendId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "FriendId");

                    b.HasIndex("FriendId");

                    b.ToTable("UserFriends");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.Group", b =>
                {
                    b.HasOne("Quantum.GroupFolder.Models.GroupRequest", "GroupRequest")
                        .WithOne("Group")
                        .HasForeignKey("Quantum.GroupFolder.Models.Group", "GroupRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupRequest");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.GroupRequestUserInfoOutput", b =>
                {
                    b.HasOne("Quantum.GroupFolder.Models.GroupRequest", "GroupRequest")
                        .WithMany("Users")
                        .HasForeignKey("GroupRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Quantum.UserP.Models.UserInfoOutput", "UserInfoOutput")
                        .WithMany("GroupRequests")
                        .HasForeignKey("UserInfoOutputId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupRequest");

                    b.Navigation("UserInfoOutput");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.GroupUserRole", b =>
                {
                    b.HasOne("Quantum.UserP.Models.User", null)
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Quantum.UserP.Models.UserGroups", b =>
                {
                    b.HasOne("Quantum.GroupFolder.Models.Group", "Group")
                        .WithMany("Members")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Quantum.UserP.Models.User", "User")
                        .WithMany("Groups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("UserFriends", b =>
                {
                    b.HasOne("Quantum.UserP.Models.UserInfoOutput", "Friend")
                        .WithMany("Friend")
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Quantum.UserP.Models.User", "User")
                        .WithMany("Friend")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Friend");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.Group", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Quantum.GroupFolder.Models.GroupRequest", b =>
                {
                    b.Navigation("Group")
                        .IsRequired();

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Quantum.UserP.Models.User", b =>
                {
                    b.Navigation("Friend");

                    b.Navigation("Groups");

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("Quantum.UserP.Models.UserInfoOutput", b =>
                {
                    b.Navigation("Friend");

                    b.Navigation("GroupRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
