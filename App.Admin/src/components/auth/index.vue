<template>
	<slot v-if="isAuth" />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { definePropType } from "@fast-china/utils";
import { useUserInfo } from "@/stores";

defineOptions({
	name: "Auth",
	options: {
		virtualHost: true,
		addGlobalClass: true,
		styleIsolation: "shared",
	},
});

const props = defineProps({
	/** 权限编码；数组中任意一项命中即可显示 */
	code: {
		type: definePropType<string | string[]>([String, Array]),
		required: true,
	},
});

const userInfoStore = useUserInfo();

const isAuth = computed(() => {
	if (userInfoStore.isSuperAdmin) return true;
	const codes = Array.isArray(props.code) ? props.code : [props.code];
	return codes.some((code) => userInfoStore.buttonCodeList.includes(code));
});
</script>
