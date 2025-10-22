using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Services;
using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IEnhancedWorkflowService _workflowService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IEnhancedWorkflowService workflowService, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    // GET: api/workflows
    [HttpGet]
    public async Task<ActionResult<List<Models.WorkflowDefinition>>> GetAllWorkflows()
    {
        try
        {
            var workflows = await _workflowService.GetAllWorkflowsAsync();
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all workflows");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/workflows/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Models.WorkflowDefinition>> GetWorkflow(string id)
    {
        try
        {
            var workflow = await _workflowService.GetWorkflowAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }
            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/workflows
    [HttpPost]
    public async Task<ActionResult<Models.WorkflowDefinition>> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        try
        {
            var workflow = await _workflowService.CreateWorkflowAsync(request.Name, request.Description);
            return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/workflows/from-template
    [HttpPost("from-template")]
    public async Task<ActionResult<Models.WorkflowDefinition>> CreateWorkflowFromTemplate([FromBody] CreateWorkflowFromTemplateRequest request)
    {
        try
        {
            var workflow = await _workflowService.CreateWorkflowFromTemplateAsync(request.TemplateName, request.Name, request.Description);
            return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow from template");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/workflows/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<Models.WorkflowDefinition>> UpdateWorkflow(string id, [FromBody] Models.WorkflowDefinition workflow)
    {
        try
        {
            if (id != workflow.Id)
            {
                return BadRequest("Workflow ID mismatch");
            }

            var updatedWorkflow = await _workflowService.UpdateWorkflowAsync(workflow);
            return Ok(updatedWorkflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/workflows/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkflow(string id)
    {
        try
        {
            var result = await _workflowService.DeleteWorkflowAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/workflows/templates
    [HttpGet("templates")]
    public ActionResult<List<string>> GetAvailableTemplates()
    {
        try
        {
            var templates = _workflowService.GetAvailableTemplates();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available templates");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/workflows/templates/{templateName}
    [HttpGet("templates/{templateName}")]
    public ActionResult<Models.WorkflowDefinition> GetWorkflowTemplate(string templateName)
    {
        try
        {
            var template = _workflowService.GetWorkflowTemplate(templateName);
            if (template == null)
            {
                return NotFound($"Template '{templateName}' not found");
            }
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow template {TemplateName}", templateName);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/workflows/{id}/mermaid
    [HttpGet("{id}/mermaid")]
    public async Task<ActionResult<string>> GenerateMermaidDiagram(string id)
    {
        try
        {
            var mermaid = await _workflowService.GenerateMermaidDiagramAsync(id);
            return Ok(new { mermaid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Mermaid diagram for workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/workflows/validate/{id}
    [HttpPost("validate/{id}")]
    public async Task<ActionResult<List<string>>> ValidateWorkflow(string id)
    {
        try
        {
            var validationErrors = await _workflowService.ValidateWorkflowAsync(id);
            return Ok(validationErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class CreateWorkflowRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class CreateWorkflowFromTemplateRequest
{
    public string TemplateName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}
