using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Seed
{
    public static class CategorySeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(

                // =========================
                // ROOT CATEGORIES
                // =========================
                new Category { Id = 1, Name = "Keyboards", Slug = "keyboards", IsActive = true },
                new Category { Id = 2, Name = "Mice", Slug = "mice", IsActive = true },
                new Category { Id = 3, Name = "Headsets", Slug = "headsets", IsActive = true },
                new Category { Id = 4, Name = "Monitors", Slug = "monitors", IsActive = true },
                new Category { Id = 5, Name = "PC Components", Slug = "pc-components", IsActive = true },
                new Category { Id = 6, Name = "Gaming Furniture", Slug = "gaming-furniture", IsActive = true },
                new Category { Id = 7, Name = "Setup Accessories", Slug = "setup-accessories", IsActive = true },
                new Category { Id = 8, Name = "Console & Controllers", Slug = "console-controllers", IsActive = true },

                // =========================
                // CHILD - KEYBOARDS
                // =========================
                new Category { Id = 11, ParentId = 1, Name = "Mechanical Keyboards", Slug = "mechanical-keyboards", IsActive = true },
                new Category { Id = 12, ParentId = 1, Name = "Membrane Keyboards", Slug = "membrane-keyboards", IsActive = true },
                new Category { Id = 13, ParentId = 1, Name = "Keycaps", Slug = "keycaps", IsActive = true },
                new Category { Id = 14, ParentId = 1, Name = "Keyboard Switches", Slug = "keyboard-switches", IsActive = true },

                // =========================
                // CHILD - MICE
                // =========================
                new Category { Id = 21, ParentId = 2, Name = "Gaming Mice", Slug = "gaming-mice", IsActive = true },
                new Category { Id = 22, ParentId = 2, Name = "Office Mice", Slug = "office-mice", IsActive = true },
                new Category { Id = 23, ParentId = 2, Name = "Mouse Pads", Slug = "mouse-pads", IsActive = true },

                // =========================
                // CHILD - HEADSETS
                // =========================
                new Category { Id = 31, ParentId = 3, Name = "Gaming Headsets", Slug = "gaming-headsets", IsActive = true },
                new Category { Id = 32, ParentId = 3, Name = "Wireless Headphones", Slug = "wireless-headphones", IsActive = true },
                new Category { Id = 33, ParentId = 3, Name = "Microphones", Slug = "microphones", IsActive = true },

                // =========================
                // CHILD - MONITORS
                // =========================
                new Category { Id = 41, ParentId = 4, Name = "Gaming Monitors", Slug = "gaming-monitors", IsActive = true },
                new Category { Id = 42, ParentId = 4, Name = "Office Monitors", Slug = "office-monitors", IsActive = true },
                new Category { Id = 43, ParentId = 4, Name = "Curved Monitors", Slug = "curved-monitors", IsActive = true },

                // =========================
                // CHILD - PC COMPONENTS
                // =========================
                new Category { Id = 51, ParentId = 5, Name = "CPUs", Slug = "cpus", IsActive = true },
                new Category { Id = 52, ParentId = 5, Name = "GPUs", Slug = "gpus", IsActive = true },
                new Category { Id = 53, ParentId = 5, Name = "RAM", Slug = "ram", IsActive = true },
                new Category { Id = 54, ParentId = 5, Name = "Motherboards", Slug = "motherboards", IsActive = true },
                new Category { Id = 55, ParentId = 5, Name = "Storage (SSD/HDD)", Slug = "storage", IsActive = true },
                new Category { Id = 56, ParentId = 5, Name = "Power Supplies", Slug = "power-supplies", IsActive = true },
                new Category { Id = 57, ParentId = 5, Name = "PC Cases", Slug = "pc-cases", IsActive = true }
            );
        }
    }
}
