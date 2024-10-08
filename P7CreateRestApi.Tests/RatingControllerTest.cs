﻿using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using P7CreateRestApi.Controllers;
using P7CreateRestApi.Domain;
using P7CreateRestApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace P7CreateRestApi.Tests
{
    public class RatingControllerTests
    {
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly Mock<ILogger<RatingController>> _loggerMock;
        private readonly RatingController _controller;

        public RatingControllerTests()
        {
            // Initialisation des mocks et du contrôleur
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _loggerMock = new Mock<ILogger<RatingController>>();
            _controller = new RatingController(_ratingRepositoryMock.Object, _loggerMock.Object);
        }

           

        // Test pour CreateRating - Cas de succès
        [Fact]
        public async Task CreateRating_ShouldReturnCreated_WhenRatingIsValid()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var rating = new Rating { Id = 1, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };
            _ratingRepositoryMock.Setup(r => r.CreateRatingAsync(rating)).ReturnsAsync(rating);  // Configure le mock pour retourner le rating

            // Act - Exécute la méthode à tester
            var result = await _controller.CreateRating(rating);

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);  // Vérifie que le résultat est du type CreatedAtActionResult (HTTP 201 Created)
            Assert.Equal(nameof(_controller.GetRatingById), actionResult.ActionName);  // Vérifie que le nom de l'action est correct
            Assert.Equal(rating.Id, ((Rating)actionResult.Value).Id);  // Vérifie que l'ID du rating est correct
        }
        // Test pour CreateRating - Cas où Rating est null
        [Fact]
        public async Task CreateRating_ShouldReturnBadRequest_WhenRatingIsNull()
        {
            // Arrange
            Rating rating = null;

            // Act
            var result = await _controller.CreateRating(rating);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // Vérifie que la réponse est de type BadRequestObjectResult (réponse 400 Bad Request avec des détails)
            Assert.Equal(400, badRequestResult.StatusCode);
            // Vérifie que le code de statut est 400

            // Assurez-vous que l'objet retourné contient le message d'erreur approprié
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("Rating object is null", errorMessage.Trim());
            // Vérifie que le message d'erreur est "Rating object is null" en supprimant les espaces superflus
        }

        // Test pour CreateRating - Cas où le ModelState est invalide
        [Fact]
        public async Task CreateRating_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var rating = new Rating { Id = 1, SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };
            _controller.ModelState.AddModelError("MoodysRating", "MoodysRating is required");

            // Act - Exécute la méthode à tester
            var result = await _controller.CreateRating(rating);

            // Assert - Vérifie que le résultat est correct
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);  // Vérifie que le résultat est du type BadRequestObjectResult (HTTP 400 Bad Request)
            Assert.True(_controller.ModelState.ContainsKey("MoodysRating"));  // Vérifie que l'erreur de validation est correcte
        }

        // Test pour CreateRating - Cas d'exception
        [Fact]
        public async Task CreateRating_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var rating = new Rating { Id = 1, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };
            _ratingRepositoryMock.Setup(r => r.CreateRatingAsync(rating)).ThrowsAsync(new Exception("Test Exception"));  // Configure le mock pour lancer une exception

            // Act - Exécute la méthode à tester
            var result = await _controller.CreateRating(rating);

            // Assert - Vérifie que le résultat est correct
            var statusCodeResult = Assert.IsType<ObjectResult>(result);  // Vérifie que le résultat est du type ObjectResult (HTTP 500 Internal Server Error)
            Assert.Equal(500, statusCodeResult.StatusCode);  // Vérifie que le code de statut est 500
            Assert.Equal("An error occurred while retrieving Rating", statusCodeResult.Value);  // Vérifie que le message d'erreur est correct
        }

        // Test pour GetRatingById - Cas où Rating est trouvé et retourné avec succès
        [Fact]
        public async Task GetRatingById_ShouldReturnOk_WhenRatingExists()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var rating = new Rating { Id = 1, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };
            _ratingRepositoryMock.Setup(r => r.GetRatingByIdAsync(1)).ReturnsAsync(rating);  // Configure le mock pour retourner le rating

            // Act - Exécute la méthode à tester
            var result = await _controller.GetRatingById(1);

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<OkObjectResult>(result);  // Vérifie que le résultat est du type OkObjectResult (HTTP 200 OK)
            var returnedRating = Assert.IsType<Rating>(actionResult.Value);  // Vérifie que la valeur retournée est du type Rating
            Assert.Equal(rating.Id, returnedRating.Id);  // Vérifie que l'ID du rating est correct
            Assert.Equal(rating.MoodysRating, returnedRating.MoodysRating);  // Vérifie que le MoodysRating est correct
            Assert.Equal(rating.SandPRating, returnedRating.SandPRating);  // Vérifie que le SandPRating est correct
            Assert.Equal(rating.FitchRating, returnedRating.FitchRating);  // Vérifie que le FitchRating est correct
        }

        // Test pour GetRatingById - Cas où aucune entité Rating n'est trouvée
        [Fact]
        public async Task GetRatingById_ShouldReturnNotFound_WhenRatingDoesNotExist()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            _ratingRepositoryMock.Setup(r => r.GetRatingByIdAsync(1)).ReturnsAsync((Rating)null);  // Configure le mock pour retourner null

            // Act - Exécute la méthode à tester
            var result = await _controller.GetRatingById(1);

            // Assert - Vérifie que le résultat est correct
            Assert.IsType<NotFoundResult>(result);  // Vérifie que le résultat est du type NotFoundResult (HTTP 404 Not Found)
        }

        // Test pour GetRatingById - Cas où une exception est lancée
        [Fact]
        public async Task GetRatingById_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            _ratingRepositoryMock.Setup(r => r.GetRatingByIdAsync(1)).ThrowsAsync(new Exception("Database error"));  // Configure le mock pour lancer une exception

            // Act - Exécute la méthode à tester
            var result = await _controller.GetRatingById(1);

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<ObjectResult>(result);  // Vérifie que le résultat est du type ObjectResult (HTTP 500 Internal Server Error)
            Assert.Equal(500, actionResult.StatusCode);  // Vérifie que le code de statut est 500
            Assert.Equal("An error occurred while retrieving Rating", actionResult.Value);  // Vérifie que le message d'erreur est correct
        }

        // Test pour GetAllRatings - Cas où des entités Rating sont trouvées et retournées avec succès
        [Fact]
        public async Task GetAllRatings_ShouldReturnOk_WhenRatingsExist()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var curves = new List<Rating>
            {
            new Rating { Id = 1, MoodysRating = "Baa", SandPRating = "BBB", FitchRating = "BBB", OrderNumber = 1 },
            new Rating{ Id = 2, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 }
            };

            _ratingRepositoryMock.Setup(r => r.GetAllRatingsAsync()).ReturnsAsync(curves);  // Configure le mock pour retourner la liste de courbes

            // Act - Exécute la méthode à tester
            var result = await _controller.GetAllRatings();

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<OkObjectResult>(result);  // Vérifie que le résultat est du type OkObjectResult (HTTP 200 OK)
            var returnedRatings = Assert.IsType<List<Rating>>(actionResult.Value);  // Vérifie que la valeur retournée est du type List<Rating>
            Assert.Equal(curves.Count, returnedRatings.Count);  // Vérifie que le nombre de courbes retournées est correct
        }

        // Test pour GetAllRatings - Cas où aucune entité Rating n'est trouvée
        [Fact]
        public async Task GetAllRatings_ShouldReturnNotFound_WhenNoRatingsExist()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            _ratingRepositoryMock.Setup(r => r.GetAllRatingsAsync()).ReturnsAsync(new List<Rating>());  // Configure le mock pour retourner une liste vide

            // Act - Exécute la méthode à tester
            var result = await _controller.GetAllRatings();

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<NotFoundResult>(result);  // Vérifie que le résultat est du type NotFoundResult (HTTP 404 Not Found)
        }

        // Test pour GetAllRatings - Cas où une exception est lancée
        [Fact]
        public async Task GetAllRatings_ShouldReturnErrorMessage_WhenExceptionIsThrown()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            _ratingRepositoryMock.Setup(r => r.GetAllRatingsAsync()).ThrowsAsync(new Exception("Database error"));  // Configure le mock pour lancer une exception

            // Act - Exécute la méthode à tester
            var result = await _controller.GetAllRatings();

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<ObjectResult>(result);  // Vérifie que le résultat est du type ObjectResult (HTTP 500 Internal Server Error)
            Assert.Equal(500, actionResult.StatusCode);  // Vérifie que le code de statut est 500
            Assert.Equal("An error occurred while retrieving all Ratings", actionResult.Value);  // Vérifie que le message d'erreur est correct
        }
        // Test pour UpdateRating - La mise à jour réussit
        [Fact]
        public async Task UpdateRating_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var originalRating = new Rating { Id = 1, MoodysRating = "Baa", SandPRating = "BBB", FitchRating = "BBB", OrderNumber = 1 };
            var updatedRating = new Rating { Id = 1, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };

            // Simule que l'entité existe
            _ratingRepositoryMock.Setup(r => r.GetRatingByIdAsync(1)).ReturnsAsync(originalRating);
            // Simule la mise à jour réussie
            _ratingRepositoryMock.Setup(r => r.UpdateRatingAsync(updatedRating)).ReturnsAsync(updatedRating);

            // Act - Exécute la méthode à tester
            var result = await _controller.UpdateRating(1, updatedRating);

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<OkObjectResult>(result);  // Vérifie que le résultat est du type OkObjectResult (HTTP 200 OK)
            var returnedRating = Assert.IsType<Rating>(actionResult.Value);  // Vérifie que la valeur retournée est du type Rating
            Assert.Equal(updatedRating.Id, returnedRating.Id);  // Vérifie que l'ID du rating est correct
            Assert.Equal(updatedRating.MoodysRating, returnedRating.MoodysRating);  // Vérifie que le MoodysRating est correct
            Assert.Equal(updatedRating.SandPRating, returnedRating.SandPRating);  // Vérifie que le SandPRating est correct
            Assert.Equal(updatedRating.FitchRating, returnedRating.FitchRating);  // Vérifie que le FitchRating est correct
        }

        // Test pour UpdateRating - Rating est nul
        [Fact]
        public async Task UpdateRating_ShouldReturnBadRequest_WhenRatingIsNull()
        {
            // Arrange
            Rating rating = null;

            // Act
            var result = await _controller.UpdateRating(1, rating);

            // Assert
            // Vérifie que le résultat est de type BadRequestObjectResult.
            // La méthode UpdateRating doit retourner un BadRequest avec un message d'erreur si l'objet Rating est null.
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Vérifie que le code de statut HTTP est 400.
            // C'est le code de réponse standard pour indiquer une demande incorrecte due à des données invalides (dans ce cas, Rating est null).
            Assert.Equal(400, badRequestResult.StatusCode);

            // Vérifie que le message de la réponse est "Rating object is null".
            // Ce message est celui qui est renvoyé par la méthode UpdateRating pour indiquer que l'objet Rating ne peut pas être null.
            Assert.Equal("Rating object is null", badRequestResult.Value);
        }

        // Test pour UpdateRating - ID ne correspond pas
        [Fact]
        public async Task UpdateRating_ShouldReturnBadRequest_WhenIdDoesNotMatch()
        {
            // Arrange
            var rating = new Rating { Id = 2, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };

            // Act
            var result = await _controller.UpdateRating(1, rating);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);  // Vérifie que le résultat est du type BadRequestObjectResult (HTTP 400 Bad Request)
            Assert.True(badRequestResult.Value is SerializableError);
            var errors = badRequestResult.Value as SerializableError;

            Assert.True(errors.ContainsKey("IdMismatch"));  // Vérifie que le modèle d'état contient une clé "IdMismatch"
            var errorMessages = errors["IdMismatch"] as string[];
            Assert.Contains("The rating ID in the URL does not match the ID in the rating object.", errorMessages);  // Vérifie le message d'erreur spécifique
        }

        // Test pour UpdateRating - L'élément à mettre à jour n'existe pas
        [Fact]
        public async Task UpdateRating_ShouldReturnNotFound_WhenRatingDoesNotExist()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var rating = new Rating { Id = 1, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };
            _ratingRepositoryMock.Setup(r => r.GetRatingByIdAsync(1)).ReturnsAsync((Rating)null);

            // Act - Exécute la méthode à tester
            var result = await _controller.UpdateRating(1, rating);

            // Assert - Vérifie que le résultat est correct
            Assert.IsType<NotFoundResult>(result);  // Vérifie que le résultat est du type NotFoundResult (HTTP 404 Not Found)
        }

        // Test pour UpdateRating - Une exception est levée
        [Fact]
        public async Task UpdateRating_ShouldReturnErrorMessage_WhenExceptionIsThrown()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            var rating = new Rating { Id = 1, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AAA", OrderNumber = 1 };
            _ratingRepositoryMock.Setup(r => r.GetRatingByIdAsync(1)).ReturnsAsync(rating);
            _ratingRepositoryMock.Setup(r => r.UpdateRatingAsync(rating)).ThrowsAsync(new Exception("Database error"));

            // Act - Exécute la méthode à tester
            var result = await _controller.UpdateRating(1, rating);

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<ObjectResult>(result);  // Vérifie que le résultat est du type ObjectResult (HTTP 500 Internal Server Error)
            Assert.Equal(500, actionResult.StatusCode);  // Vérifie que le code de statut est 500
            Assert.Equal("UpdateRating: An error occurred while updating the rating with ID", actionResult.Value);  // Vérifie que le message d'erreur est correct
        }

        // Test pour DeleteRating - Suppression réussie
        [Fact]
        public async Task DeleteRating_ShouldReturnNoContent_WhenDeleteIsSuccessful()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            int ratingId = 1;
            _ratingRepositoryMock.Setup(r => r.DeleteRatingAsync(ratingId)).ReturnsAsync(true);

            // Act - Exécute la méthode à tester
            var result = await _controller.DeleteRating(ratingId);

            // Assert - Vérifie que le résultat est correct
            Assert.IsType<NoContentResult>(result);  // Vérifie que le résultat est NoContentResult (HTTP 204 No Content)
        }

        // Test pour DeleteRating - Rating non trouvé
        [Fact]
        public async Task DeleteRating_ShouldReturnNotFound_WhenRatingDoesNotExist()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            int ratingId = 1;
            _ratingRepositoryMock.Setup(r => r.DeleteRatingAsync(ratingId)).ReturnsAsync(false);

            // Act - Exécute la méthode à tester
            var result = await _controller.DeleteRating(ratingId);

            // Assert - Vérifie que le résultat est correct
            Assert.IsType<NotFoundResult>(result);  // Vérifie que le résultat est NotFoundResult (HTTP 404 Not Found)
        }

        // Test pour DeleteRating - Exception levée
        [Fact]
        public async Task DeleteRating_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange - Prépare les objets nécessaires pour le test
            int ratingId = 1;
            _ratingRepositoryMock.Setup(r => r.DeleteRatingAsync(ratingId)).ThrowsAsync(new Exception("Delete failed"));

            // Act - Exécute la méthode à tester
            var result = await _controller.DeleteRating(ratingId);

            // Assert - Vérifie que le résultat est correct
            var actionResult = Assert.IsType<ObjectResult>(result);  // Vérifie que le résultat est du type ObjectResult (HTTP 500 Internal Server Error)
            Assert.Equal(500, actionResult.StatusCode);  // Vérifie que le code de statut est 500
            Assert.Equal("An error occurred while retrieving Rating", actionResult.Value);  // Vérifie que le message d'erreur est correct
        }
    }
}
