<script>
  import Button from "../components/Button.svelte";

  let className = "";
  export { className as class };
</script>

<div class={className}>
  <Button>Buy</Button>
</div>
