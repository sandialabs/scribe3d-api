using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSAS_Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GSAS_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ScribeController : ControllerBase
    {


        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private GSAS_Context _context;

        public ScribeController(GSAS_Context context, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _config = configuration;
            _context = context;
        }
        /// <summary>
        /// POST creates a new session
        /// https://localhost:44374/api/scribe/CreateSession 
        /// Dictionary key value pair { "sessionInfo", {serialized ScribeSession} }
        /// </summary>
        /// <param name="sessionInfo">Need name, password, hostId, scribe save file</param>
        /// <returns>session id</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateSession([FromBody] ScribeSession session)
        {
            await _context.ScribeSession.AddAsync(session);
            await _context.SaveChangesAsync();

            return Ok(session.Id);
        }
        /// <summary>
        /// POSTs updated scribe session. HQ only
        /// </summary>
        /// <param name="sessionInfo">session info with id, password,  and scribe save file</param>
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateSession([FromBody] ScribeSession sessionInfo)
        {
            var session = await _context.ScribeSession.FindAsync(sessionInfo.Id);
            if (session == null) return NotFound();
            if (session.Password != sessionInfo.Password) return Unauthorized("Invalid Password");
            //if (session.HostId != sessionInfo.hostId) return Unauthorized("Invalid Host Id");
            session.SaveFile = sessionInfo.SaveFile;
            session.CurrentTeam = sessionInfo.CurrentTeam;
            session.PlaySpeed = sessionInfo.PlaySpeed;
            session.GameTime = sessionInfo.GameTime;
            session.SaveFileId++;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// POSTs updated team using ScribeSession container
        /// </summary>
        /// <param name="sessionInfo">session info with only current team set</param>
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateTeam([FromBody] ScribeSession sessionInfo)
        {
            var session = await _context.ScribeSession.FindAsync(sessionInfo.Id);
            if (session == null) return NotFound();
            if (session.Password != sessionInfo.Password) return Unauthorized("Invalid Password");
            //if (session.HostId != sessionInfo.hostId) return Unauthorized("Invalid Host Id");
            session.CurrentTeam = sessionInfo.CurrentTeam;
            await _context.SaveChangesAsync();

            return Ok();
        }



        /// <summary>
        /// GET list of active ScribeSessions (only includes name, id, createDate)
        /// </summary>
        /// <returns>list of active scribe sessions</returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var activeSessions = await _context.ScribeSession.Where(w => w.IsActive).OrderBy(w => w.CreateDate).Select(w => new { w.SessionName, w.Id, w.CreateDate }).ToListAsync();
            return Ok(activeSessions);
        }
        /// <summary>
        /// Deactivates session so it's not visible to search
        /// </summary>
        /// <param name="sessionId">Id of session, sesion Id, this is the sessions Id. Id stands for identification.. I'm pretty sure at least. Nothing else it could really stand for</param>
        [HttpDelete("[action]/{sessionId}")]
        public async Task<IActionResult> DeactivateSession(int sessionId)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(sessionId);
            if (session == null) return NotFound();
            session.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// GETs the full session with scribe save file
        /// </summary>
        /// <param name="sessionInfo">Just need id and password</param>
        /// <returns>full session without hostId</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> GetSession([FromBody] ScribeSession sessionInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(sessionInfo.Id);
            if (session == null) return NotFound();
            if (session.Password != sessionInfo.Password) return Unauthorized("Invalid Password");
            session.HostId = null;
            return Ok(session);

        }

        /// <summary>
        /// GETs the current team of the session
        /// </summary>
        [HttpGet("[action]/{sessionId}")]
        public async Task<IActionResult> GetCurrentTeam([FromRoute] int sessionId)
        {
            var session = await _context.ScribeSession.FindAsync(sessionId);
            if (session == null) return NotFound();
            return Ok(session.CurrentTeam);
        }

        /// <summary>
        /// GETs the current team of the session
        /// </summary>
        [HttpGet("[action]/{sessionId}")]
        public async Task<IActionResult> GetLatestProposedchange([FromRoute] int sessionId)
        {
            var session = await _context.ScribeSession.FindAsync(sessionId);
            if (session == null) return NotFound();
            try
            {
                List<ScribeProposedChange> sessionProposedChanges = _context.ScribeProposedChange.Where(w => w.SessionId == session.Id).ToList();
                if (sessionProposedChanges != null && sessionProposedChanges.Count > 0)
                {
                    var change = sessionProposedChanges.LastOrDefault();
                    return Ok(change);
                }
                else
                {
                    return Ok(null);
                }
            }
            catch (ArgumentNullException e) {
                return Ok(null);
            }
        }

        /// <summary>
        /// GETs any pending changes that have not been approved or rejected. Returns NotFound if 
        /// one doesn't currently exist
        /// </summary>
        [HttpGet("[action]/{sessionId}")]
        public async Task<IActionResult> GetLatestPendingProposedChange([FromRoute] int sessionId)
        {
            var session = await _context.ScribeSession.FindAsync(sessionId);
            if (session == null) return NotFound();
            try
            {
                List<ScribeProposedChange> sessionProposedChanges = _context.ScribeProposedChange.Where(w => w.SessionId == session.Id).ToList();
                if (sessionProposedChanges != null && sessionProposedChanges.Count > 0)
                {
                    var change = sessionProposedChanges.LastOrDefault();
                    if(!change.IsApproved && !change.IsDenied)
                    {
                        return Ok(change);
                    }
                    else return NotFound();
                }
                else
                {
                    return NotFound(); ;
                }
            }
            catch (ArgumentNullException e)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// GETs the current team of the session
        /// </summary>
        [HttpGet("[action]/{sessionId}")]
        public async Task<IActionResult> GetSessionActiveStatus([FromRoute] int sessionId)
        {
            var session = await _context.ScribeSession.FindAsync(sessionId);
            if (session == null) return NotFound(false);
            return Ok(session.IsActive);
        }


        /// <summary>
        /// GETs whether the save file has changed. Pollable function
        /// Example: https://localhost:44374/api/scribe/HasSessionChanged/22/3
        /// </summary>
        /// <param name="sessionId">ROUTE Id of session</param>
        /// <param name="saveFileId">ROUTE Id of saveFile</param>
        /// <returns>boolean if session changed</returns>
        [HttpGet("[action]/{sessionId}/{saveFileId}")] 
        public async Task<IActionResult> HasSessionChanged([FromRoute] int sessionId, [FromRoute] int saveFileId)
        {
            int scenCount = await _context.ScribeSession.Where(w => w.Id == sessionId && w.SaveFileId == saveFileId).CountAsync();
            bool hasSessionChanged = scenCount == 0; 
            return Ok(hasSessionChanged);

        }
        /// <summary>
        /// POST proposed change to server for host to look over.
        /// </summary>
        /// <param name="proposedChangeInfo">BODY proposed change with password</param>
        /// <returns>proposed change id</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> ProposeChange([FromBody] ProposedChangeInfo proposedChangeInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(proposedChangeInfo.proposedChange.SessionId);
            if (session == null) return NotFound();
            if (session.Password != proposedChangeInfo.password) return Unauthorized("Invalid Password");
            proposedChangeInfo.proposedChange.IsApproved = false;
            proposedChangeInfo.proposedChange.IsDenied = false;

            await _context.ScribeProposedChange.AddAsync(proposedChangeInfo.proposedChange);
            await _context.SaveChangesAsync();

            if(proposedChangeInfo?.proposedChange != null  && proposedChangeInfo.proposedChange.Team == "HQ")
            {
                await ApproveChange(proposedChangeInfo);
            }

            return Ok(proposedChangeInfo.proposedChange.Id);
        }

        /// <summary>
        /// GET feedback from host on if the change was approved or denied and the hosts message
        /// </summary>
        /// <param name="changeId">ROUTE Id of proposed change</param>
        /// <returns>ProposedChangeInfo but just IsApproved, IsDenied, HostMessage</returns>
        [HttpGet("[action]/{changeId}")]
        public async Task<IActionResult> PollProposedChange([FromRoute] int changeId)
        {
            var change = await _context.ScribeProposedChange.Where(w => w.Id == changeId).Select(w => new { w.IsApproved, w.IsDenied, w.HostMessage }).FirstOrDefaultAsync();
            if (change == null) return NotFound();

            return Ok(change);

        }

        /// <summary>
        /// POST returns all changes that have been approved for a given session
        /// </summary>
        /// <param name="changeInfo">BODY change info</param>
        /// <returns>List of proposed changes that have been approved. Null if none</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> GetAllApprovedChanges([FromBody] ScribeSession scribeUserSession)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(scribeUserSession.Id);
            if (session == null) return NotFound();
            if (session.Password != scribeUserSession.Password) return Unauthorized("Invalid Password");
            var approvedChanges = await _context.ScribeProposedChange.Where(w => w.SessionId == session.Id && w.IsApproved).Select(w => new { w.Id, w.Team, w.Description, w.HostMessage }).ToListAsync();
            return Ok(approvedChanges);
        }

        /// <summary>
        /// POST returns the full change object given the id of a change object
        /// </summary>
        /// <param name="scribeUserSession">BODY change info</param>
        /// <returns>List of proposed changes that have been approved. Null if none</returns>
        [HttpPost("[action]/{changeId}")]
        public async Task<IActionResult> GetChangeDetailsByID([FromRoute] int changeId, [FromBody] ScribeSession scribeUserSession)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(scribeUserSession.Id);
            if (session == null) return NotFound();
            if (session.Password != scribeUserSession.Password) return Unauthorized("Invalid Password");
            var approvedChange = await _context.ScribeProposedChange.FirstOrDefaultAsync(entry => entry.Id == changeId);
            return Ok(approvedChange);
        }

        /// <summary>
        /// GET last change from a team
        /// </summary>
        /// <param name="changeInfo">BODY change info</param>
        /// <returns>new proposed change for team. Null if none</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> GetProposedChange([FromBody] HostProposedChange changeInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(changeInfo.sessionId);
            if (session == null) return NotFound();
            if (session.Password != changeInfo.password) return Unauthorized("Invalid Password");
            //if (session.HostId != changeInfo.hostId) return Unauthorized("Invalid Host Id");
            var change = await _context.ScribeProposedChange.Where(w => w.SessionId == session.Id && w.Team == changeInfo.team && w.IsApproved == false && w.IsDenied == false).FirstOrDefaultAsync();
            return Ok(change);

        }

        /// <summary>
        /// POST HQ approves the change
        /// </summary>
        /// <param name="proposedChangeInfo">updated proposed change</param>
        [HttpPost("[action]")]
        public async Task<IActionResult> ApproveChange([FromBody] ProposedChangeInfo proposedChangeInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(proposedChangeInfo.proposedChange.SessionId);
            if (session == null) return NotFound();
            if (session.Password != proposedChangeInfo.password) return Unauthorized("Invalid Password");
            //if (session.HostId != proposedChangeInfo.hostId) return Unauthorized("Invalid Host Id");
            proposedChangeInfo.proposedChange.IsApproved = true;
            proposedChangeInfo.proposedChange.IsDenied = false;
            _context.ScribeProposedChange.Update(proposedChangeInfo.proposedChange);
            session.SaveFile = proposedChangeInfo.proposedChange.ScribeSaveFile;
            session.SaveFileId++;
            await _context.SaveChangesAsync();

            return Ok();
        }
        /// <summary>
        /// POST HQ denies the change
        /// </summary>
        /// <param name="proposedChangeInfo">updated proposed change</param>
        [HttpPost("[action]")]
        public async Task<IActionResult> DenyChange([FromBody] ProposedChangeInfo proposedChangeInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(proposedChangeInfo.proposedChange.SessionId);
            if (session == null) return NotFound();
            if (session.Password != proposedChangeInfo.password) return Unauthorized("Invalid Password");
            //if (session.HostId != proposedChangeInfo.hostId) return Unauthorized("Invalid Host Id");
            proposedChangeInfo.proposedChange.IsApproved = false;
            proposedChangeInfo.proposedChange.IsDenied = true;
            _context.ScribeProposedChange.Update(proposedChangeInfo.proposedChange);
            await _context.SaveChangesAsync();

            return Ok();
        }
        /// <summary>
        /// POSTS user message to users
        /// </summary>
        /// <param name="scribeMessageInfo">message with password info</param>
        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage([FromBody] ScribeMessageInfo scribeMessageInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(scribeMessageInfo.scribeMessage.SessionId);
            if (session == null) return NotFound();
            if (session.Password != scribeMessageInfo.password) return Unauthorized("Invalid Password");
            scribeMessageInfo.scribeMessage.TimeSent = DateTimeOffset.Now;
            await _context.ScribeMessage.AddAsync(scribeMessageInfo.scribeMessage);
            await _context.SaveChangesAsync();

            return Ok();
        }


        /// <summary>
        /// GETs new messages based on message count to/from team
        /// </summary>
        /// <param name="messageCount">ROUTE - The number of messages client currently has sent to them</param>
        /// <param name="sessionInfo">BODY - Password and session id needed</param>
        /// <returns>List of new messages</returns>
        [HttpPost("[action]/{messageCount}/{team}")]
        public async Task<IActionResult> GetNewMessages([FromRoute] int messageCount, [FromRoute] string team, [FromBody] ScribeSession sessionInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(sessionInfo.Id);
            if (session == null) return NotFound();
            if (session.Password != sessionInfo.Password) return Unauthorized("Invalid Password");

            var messageQuery = _context.ScribeMessage.Where(w => w.SessionId == sessionInfo.Id && (w.ToTeam == team || w.FromTeam == team || w.ToTeam == "All"));
            int serverCount = await messageQuery.CountAsync();
            List<ScribeMessage> newMessages = new List<ScribeMessage>();
            if(serverCount > messageCount)
            {
                newMessages = await messageQuery.OrderByDescending(w => w.TimeSent).Take(serverCount - messageCount).ToListAsync();
            }

            return Ok(newMessages);
        }


        [HttpGet("[action]/{sessionId}")]
        public async Task<IActionResult> GetTurnTime([FromRoute] int sessionId)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(sessionId);
            if (session == null) return NotFound();
            return Ok(session.TurnTime);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetTurnTime([FromBody] ScribeTurnTime turnTimeInfo)
        {
            ScribeSession session = await _context.ScribeSession.FindAsync(turnTimeInfo.sessionId);
            if (session == null) return NotFound();
            if (session.Password != turnTimeInfo.password) return Unauthorized("Invalid Password");
            session.TurnTime = turnTimeInfo.time;
            await _context.SaveChangesAsync();
            return Ok();
        }


        ///// <summary>
        ///// POSTS user into session
        ///// </summary>
        ///// <returns>UserId (int)</returns>
        //[HttpPost("[action]")]
        //public async Task<IActionResult> AddUser([FromBody] ScribeUserInfo userInfo)
        //{
        //    ScribeSession session = await _context.ScribeSession.FindAsync(userInfo.scribeUser.SessionId);
        //    if (session == null) return NotFound();
        //    if (session.Password != userInfo.password) return Unauthorized("Invalid Password");
        //    await _context.ScribeUser.AddAsync(userInfo.scribeUser);
        //    await _context.SaveChangesAsync();

        //    return Ok(userInfo.scribeUser.Id);
        //}

        ///// <summary>
        ///// Get user if exists with mac address
        ///// </summary>
        ///// <returns>User or null</returns>
        //[HttpGet("[action]/{sessionId}/{macAddress}")]
        //public async Task<IActionResult> GetUserFromMac([FromRoute] int sessionId, [FromRoute] string macAddress)
        //{
        //    ScribeUser user = await _context.ScribeUser.FirstOrDefaultAsync(w => w.SessionId == sessionId && w.MacAddress == macAddress);

        //    return Ok(User);
        //}
        ///// <summary>
        ///// Removes user with id
        ///// </summary>
        //[HttpDelete("[action]/{userId}")]
        //public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        //{
        //    ScribeUser user = await _context.ScribeUser.FindAsync(userId);
        //    if (user == null) return Unauthorized();
        //    user.IsDeleted = true;
        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}
        ///// <summary>
        ///// Gets a list of sessions users that are not deleted
        ///// </summary>
        ///// <returns>User or null</returns>
        //[HttpGet("[action]/{sessionId}")]
        //public async Task<IActionResult> GetSessionUsers([FromRoute] int sessionId)
        //{
        //    var users = await _context.ScribeUser.Where(w => w.SessionId == sessionId && w.IsDeleted == false).ToListAsync();

        //    return Ok(users);
        //}
    }

    //public class ScribeSessionInfo
    //{
    //    public int id { get; set; }
    //    public string sessionName { get; set; }
    //    public string hostId { get; set; }
    //    public string password { get; set; }
    //    public string scribeSaveFile { get; set; }

    //    public ScribeSession CreateSession()
    //    {
    //        var session = new ScribeSession();
    //        session.SessionName = this.sessionName;
    //        session.HostId = this.hostId;
    //        session.Password = this.password;
    //        session.CreateDate = DateTimeOffset.Now;
    //        session.SaveFile = scribeSaveFile;
    //        return session;
    //    }
    //}

    public class ProposedChangeInfo
    {
        public string hostId { get; set; }
        public string password { get; set; }
        public ScribeProposedChange proposedChange { get; set; }
    }
    public class HostProposedChange
    {
        public string hostId { get; set; }
        public string password { get; set; }
        public int sessionId { get; set; }
        public string team { get; set; }
    }

    public class ScribeMessageInfo
    {
        public string password { get; set; }
        public ScribeMessage scribeMessage { get; set; }
    }

    public class ScribeTurnTime
    {
        public int sessionId { get; set; }
        public string  password { get; set; }
        public float time { get; set; }
    }
    //public class ScribeUserInfo
    //{
    //    public string password { get; set; }
    //    public ScribeUser scribeUser { get; set; }
    //}
}
