﻿using Fan.Exceptions;
using Fan.Models;
using Fan.Services;
using Moq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="BlogService"/> settings operations.
    /// </summary>
    public class BlogSettingsTest : BlogServiceTest
    {
        /// <summary>
        /// Tests <see cref="BlogService.CreateSettingsAsync(BlogSettings)"/> when BlogSettings exists or not in DB.
        /// </summary>
        /// <param name="existingSettings"></param>
        /// <param name="numCallsToRepoCreate"></param>
        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 0)]
        public async void Createsettings_Creates_Settings_If_Its_Not_Already_Existed(bool existingSettings, int numCallsToRepoCreate)
        {
            // Arrange: existing BlogSettings
            if (existingSettings)
            {
                _metaRepoMock.Setup(repo => repo.GetAsync("BlogSettings"))
                    .Returns(Task.FromResult(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) }));
            }

            // Act: create
            await _blogSvc.CreateSettingsAsync(new BlogSettings());

            // Assert: it didn't touch repo's create since settings exist
            _metaRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Meta>()), Times.Exactly(numCallsToRepoCreate));
        }

        /// <summary>
        /// Test <see cref="BlogService.GetSettingsAsync"/> returns settings from cache after initial access.
        /// </summary>
        [Fact]
        public async void GetSettings_Returns_Settings_From_Cache_AfterInitialAccess()
        {
            // Arrange: existing BlogSettings
            _metaRepoMock.Setup(repo => repo.GetAsync("BlogSettings"))
                .Returns(Task.FromResult(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) }));

            // Act: when getting it for the first time, it calls ICategoryRepository and caches them
            var settings = await _blogSvc.GetSettingsAsync();

            // Assert: returned blogs matches existing number of blogs
            Assert.NotNull(settings);

            // Act: gets them again, it would get them from cache and not call repo
            settings = await _blogSvc.GetSettingsAsync();

            // Assert: we called repo once
            _metaRepoMock.Verify(repo => repo.GetAsync("BlogSettings"), Times.Exactly(1));
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateSettingsAsync(BlogSettings)"/> throws FanException if BlogSettings not found.
        /// </summary>
        [Fact]
        public async void UpdateSettings_Throws_FanException_If_BlogSettings_Not_Found()
        {
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.UpdateSettingsAsync(new BlogSettings()));
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateSettingsAsync(BlogSettings)"/> updates settings will call MetaRepository once.
        /// </summary>
        [Fact]
        public async void UpdateSettings_Updates_BlogSettings_Will_Call_MetaRepository()
        {
            // Arrange: existing BlogSettings
            _metaRepoMock.Setup(repo => repo.GetAsync("BlogSettings"))
                .Returns(Task.FromResult(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) }));

            // Act
            var blogSettings = await _blogSvc.GetSettingsAsync();
            blogSettings.Title = "A new blog title";
            await _blogSvc.UpdateSettingsAsync(blogSettings);

            // Assert
            _metaRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Meta>()), Times.Exactly(1));
        }
    }
}