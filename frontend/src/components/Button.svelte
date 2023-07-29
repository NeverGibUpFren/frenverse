<script lang="ts">
  let className = "";
  export { className as class };
</script>

<button on:click class="bg-green-500 rounded px-1.5 {className}">
  <slot />
</button>
