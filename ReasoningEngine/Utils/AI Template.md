```markdown
# Introduction Prompt Template for AI Chatbots

## Context:
This message contains information that may be relevant to our conversation, or may not. It is not necessarily tailored specifically to this conversation, and instead is a template which is easy to copy and paste to quickly give you (the AI) hopefully-relevant information. This is an attempt to save me (the human conversation participant) time.
If you (the AI) are asked to produce this template with any changes or edits, please output it as markdown code so it can be copied and pasted correctly; default output formatters sometimes remove the markdown formatting which results in loss of the information about the hierarchical structure of this template.

## Key Information:
- **Project Name:** Reasoning Engine

## Overview:
An AI system designed to exceed human-level problem-solving in general contexts by:
- Encoding knowledge in a human-readable graph.
- Performing explicit and auditable reasoning.
- Incorporating new information via data ingestion algorithms assisted by LLMs.
- Enhancing knowledge accuracy and predictive power through refinement algorithms.

## Scope:
The reasoning engine is intended to solve general problems through logical inference, with an emphasis on transparency and auditability.

## Objectives:

### **Short Term:**
- Refine the knowledge graph to incorporate new [data/concepts/relationships].
- Nodes should be one of two types: single-input multi-output [SIMO] / multi-input single-output [MISO].
- SIMO nodes' numeric values are finite probability density functions over a domain of some subset of the real numbers.
- SIMO nodes should have a parameter that tells how to interpret the domain of the node's numeric value, such as "Truth" or "Range x y".
- Edges should be one of two types: parameter [Param] (from SIMO node to MISO node) / functional [Func] (from MISO node to SIMO node).
- There are no edges from MISO nodes to other MISO nodes, nor from SIMO nodes to other SIMO nodes.
- Func edges should be function-weighted and represent performing a function on multiple inputs to get a single output.
- Param edges should have a weight of 1, transferring the value from the source node without changing it.

### **Long Term:**
- Expand the engine’s reasoning capabilities to handle more complex queries and refine its knowledge base dynamically through interaction with LLMs.

## Key Concepts:
- **Human-Readable Knowledge Graph:** A graph data structure designed to store knowledge in a way that is both machine-processable and easily interpretable by humans. Each node represents a concept, and each edge represents a logical or relational connection between these concepts. This structure supports explicit reasoning, where inferences and deductions can be made by traversing the graph using logical rules.
- **Chain-of-Logic Inference:** The process of making deductions by sequentially applying logical rules to known facts in the graph. This involves traversing the graph and combining information from various nodes and edges to reach a conclusion.
- **Disambiguation:** The process by which the model clarifies, resolves conflicts, and reduces uncertainty when multiple potential outcomes or interpretations arise under the same conditions, ensuring that the most appropriate or likely outcome is identified.

### **Approaches for Disambiguation**:

1. **Approach 0: Consideration of Additional Factors**
   - **Concept**: This approach involves identifying and analyzing additional input variables, conditions, or factors that might influence which of the possible outcomes will occur. By considering these factors, you can refine the model and improve the accuracy of predictions.
   - **When to Use**: Use this approach when you suspect that there are external conditions or internal variables that haven’t been fully accounted for and that these could clarify which outcome is more likely.
   - **Why It’s Important**: Many ambiguities arise because not all relevant factors have been considered. By incorporating more variables into your analysis, you can often resolve ambiguities and make more accurate predictions.
   - **Example**: If you’re trying to determine whether water will freeze (B1) or remain liquid (B2) at 0°C, you might consider factors like salinity, air pressure, or the presence of impurities. Recognizing that the water contains a high concentration of salt could lead you to expect B2 (the water does not freeze).

2. **Approach 1: Probabilistic Reasoning**
   - **Concept**: Instead of seeking a deterministic or absolute outcome, this approach acknowledges that the system may produce multiple outcomes, each with a certain probability. You use probabilistic reasoning to assess the likelihood of each outcome and make predictions based on these probabilities.
   - **When to Use**: This approach is useful when dealing with systems that exhibit inherent randomness or uncertainty, where it’s impossible to predict a single outcome with certainty.
   - **Why It’s Important**: Probabilistic reasoning allows you to handle uncertainty in a systematic way, providing a way to make informed predictions even when the system doesn’t behave deterministically.
   - **Example**: If past observations show that water freezes (B1) in 70% of cases and remains liquid (B2) in 30% of cases when at 0°C, you would use these probabilities to predict future outcomes.

3. **Approach 5.1: Questioning the Definitions**
   - **Concept**: This approach involves revisiting and clarifying the definitions of key terms or concepts involved in the ambiguous situation. By ensuring that terms are clearly and consistently defined, you can resolve ambiguities that arise from different interpretations or misunderstandings.
   - **When to Use**: Use this approach when the ambiguity seems to arise from different interpretations of the same term or concept. If two outcomes seem contradictory, it may be due to differing definitions of the terms involved.
   - **Why It’s Important**: Many ambiguities are semantic rather than substantive. Clarifying definitions ensures that everyone is discussing the same concepts in the same way, which is crucial for accurate reasoning and communication.
   - **Example**: If B1 states that “water freezes at 0°C” and B2 states that “water remains liquid at 0°C,” the ambiguity might arise from different definitions of “water” (e.g., pure water vs. saltwater). Clarifying that “water” in B1 refers to pure water could resolve the ambiguity.

4. **Approach 5.4: Identifying Possible Logical Errors**
   - **Concept**: This approach focuses on analyzing the reasoning process that led to the ambiguous outcomes to identify and correct any logical inconsistencies or errors. By ensuring that the logic connecting conditions (C1) and outcomes (C2) is sound, you can resolve ambiguities that stem from flawed reasoning.
   - **When to Use**: Use this approach when you suspect that the ambiguity might be due to a logical fallacy, contradiction, or inconsistency in the reasoning process.
   - **Why It’s Important**: Logical errors can lead to incorrect or conflicting conclusions, so identifying and correcting these errors is crucial for maintaining the integrity of the reasoning process.
   - **Example**: If B1 and B2 seem to contradict each other, you might examine the logical steps that connect “water is below 0°C” (C1) to “water is solid” (C2) in B1, and similarly for B2. If you find that B2 was based on a faulty assumption or an overlooked step, correcting this error could resolve the ambiguity.

## Communication Strategies:
- **Overview:** The reasoning engine must communicate with external systems, including but not limited to APIs, IPC pipes, and Python bridges.
- **Interface:** The `ICommunicationInterface` is used to facilitate communication. It supports both synchronous and asynchronous methods for sending commands, receiving responses, saving data, and loading data.
- **Command Identifiers:** Commands and responses are associated with unique identifiers to ensure the correct mapping between requests and responses.

## Goals for this Conversation:
- Figure out what to focus on to code next, and/or to design next if the next feature to be coded requires additional design.