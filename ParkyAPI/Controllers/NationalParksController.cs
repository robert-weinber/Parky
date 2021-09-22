﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Models;
using ParkyAPI.Models.Dtos;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "ParkyOpenAPISpecNP")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class NationalParksController : Controller
    {
        private readonly INationalParkRepository _npRepo;
        private readonly IMapper _mapper;
        public NationalParksController(INationalParkRepository npRepo, IMapper mapper)
        {
            _npRepo = npRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of national parks.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NationalParkDto>))]
        public IActionResult GetNationalParks()
        {
            var objList = _npRepo.GetNationalParks();
            var objDto = new List<NationalParkDto>();
            foreach (var obj in objList)
            {
                objDto.Add(_mapper.Map<NationalParkDto>(obj));
            }
            return Ok(objDto);
        }

        /// <summary>
        /// Get individual national park.
        /// </summary>
        /// <param name="nationalParkId"> The Id of the national park.</param>
        /// <returns></returns>

        [HttpGet("{nationalParkId:int}", Name = "GetNationalPark")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NationalParkDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetNationalPark(int nationalParkId)
        {
            var obj = _npRepo.GetNationalPark(nationalParkId);
            if (obj == null)
            {
                return NotFound();
            }
            var objDto = _mapper.Map<NationalParkDto>(obj);
            return Ok(objDto);
        }

        /// <summary>
        /// Create national park
        /// </summary>
        /// <param name="nationalParkDto"> National park to be added.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(NationalParkDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public IActionResult CreateNationalPark([FromBody]NationalParkDto nationalParkDto)
        {
            if(nationalParkDto == null)
            {
                return BadRequest(ModelState);
            }
            if(_npRepo.NationalParkExists(nationalParkDto.Name))
            {
                ModelState.AddModelError("", "National Park Exists!");
                return StatusCode(404, ModelState);
            }
            var nationalParkobj = _mapper.Map<NationalPark>(nationalParkDto);
            if(!_npRepo.CreateNationalPark(nationalParkobj))
            {
                ModelState.AddModelError("",$"Something went wrong when saving the record {nationalParkobj.Name}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetNationalPark",new { nationalParkId = nationalParkobj.Id },nationalParkobj);
        }

        /// <summary>
        /// Update a national park
        /// </summary>
        /// <param name="nationalParkId"> The Id of the national park.</param>
        /// <param name="nationalParkDto"> The details of the national park.</param>
        /// <returns></returns>
        [HttpPatch("{nationalParkId:int}", Name = "UpdateNationalPark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateNationalPark(int nationalParkId, [FromBody] NationalParkDto nationalParkDto)
        {

            if (nationalParkDto == null || nationalParkId != nationalParkDto.Id)
            {
                return BadRequest(ModelState);
            }
            var nationalParkobj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_npRepo.UpdateNationalPark(nationalParkobj))
            {
                ModelState.AddModelError("", $"Something went wrong when updatnig the record {nationalParkobj.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        /// <summary>
        /// Delete a national park.
        /// </summary>
        /// <param name="nationalParkId"> The Id of the national park.</param>
        /// <returns></returns>
        [HttpDelete("{nationalParkId:int}", Name = "DeleteNationalPark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {

            if (!_npRepo.NationalParkExists(nationalParkId))
            {
                return NotFound();
            }
            var nationalParkobj = _npRepo.GetNationalPark(nationalParkId);
            if (!_npRepo.DeleteNationalPark(nationalParkobj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {nationalParkobj.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
