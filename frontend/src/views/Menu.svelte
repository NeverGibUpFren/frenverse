<script>
  import Button from "../components/Button.svelte";

  let className = "";
  export { className as class };
</script>

<div class=" w-20 {className}">
  <Button class="bg-cyan-500 shadow ">Menu</Button>
</div>
